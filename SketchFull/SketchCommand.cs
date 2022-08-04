using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Structure;
using System.Windows.Forms;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Data;


namespace SketchFull
{

    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand
    /// </summary>    
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    public class SketchCommand : IExternalCommand
    {
        
        // public StreamWriter writer;        

        /// <summary>
        /// Временный вид для построения чертежа
        /// </summary>
        public static ViewSection vs = null;
        /// <summary>
        /// Список стержней с эскизами
        /// </summary>
        List<int> RebarsId = new List<int>();
        /// <summary>
        /// Список групп с эскизами
        /// </summary>
        List<int> GroupsId = new List<int>();
        /// <summary>
        /// Данные по эскизам
        /// </summary>
        List<GroupOnView> Groups = new List<GroupOnView>();
        /// <summary>
        /// Схема хранения параметров
        /// </summary>
        Schema schema_sketchs;
        Guid SchemaSketchs = new Guid("1A815B68-BA0D-4FAA-89DB-2F8E27F829B3");        
        /// <summary>
        /// Хранилище данных проекта
        /// </summary>
        DataStorage ds;
        /// <summary>
        /// Вектор расстановки эскизов в группе
        /// </summary>
        XYZ vector_groups = null;
        Document doc;
        // List<TextOnRebar> lg = new List<TextOnRebar>();
        dataManager m_data = new dataManager();

        public virtual Result Execute(ExternalCommandData commandData
           , ref string message, ElementSet elements)
        {
            // writer = new StreamWriter("e:\\time.txt");
            string VN = commandData.Application.Application.VersionNumber;
            if (Convert.ToInt32(VN) > 2023)
            {
                MessageBox.Show(Resourses.Strings.Texts.VersionNumber);
                return Result.Cancelled;
            }

            doc = commandData.Application.ActiveUIDocument.Document;

            Autodesk.Revit.ApplicationServices.LanguageType lt = doc.Application.Language;
            if (lt.ToString() == "Russian") Resourses.Strings.Texts.Culture = new System.Globalization.CultureInfo("ru-RU");

            // Некоторые виды не используются

            Type type_view = doc.ActiveView.GetType();
            if (type_view.Name == "View3D" || type_view.Name == "ViewSchedule" || type_view.Name == "ViewDrafting" || type_view.Name == "ViewSheet")
            {
                TaskDialog.Show(SketchFull.Resourses.Strings.Texts.Attension,
                                SketchFull.Resourses.Strings.Texts.Info1);
                return Result.Cancelled;
            }

            // получить данные для создания временного вида
            ElementId DetailViewId = new ElementId(-1);
            IList<Element> elems = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements();
            foreach (Element e in elems)
            {
                ViewFamilyType v = e as ViewFamilyType;

                if (v != null && v.ViewFamily == ViewFamily.Detail)
                {
                    DetailViewId = e.Id;
                    break;
                }
            }

            GetLineStyles(); // подготовить данные по стилям линий и шрифтам. Установить значение по умолчанию
            GetRebarTags();  // подготовить данные по меткам для стержней
            ReadDataFromProject(); // прочитать сохраненные данные проекта
                       

            Settings initial_form = new Settings(m_data);
            DialogResult DR = initial_form.ShowDialog();
            if (m_data.Scale <= 0) m_data.Scale = 1.0;
           
            if (DR == DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            

            if (DR == DialogResult.Yes)
            {
                UpdateSketchs();
                return Result.Succeeded;
            }

            if (DR == DialogResult.No)
            {
                UpdateSketchsOnActiveView();
                return Result.Succeeded;
            }

            Transaction t_create = new Transaction(doc,SketchFull.Resourses.Strings.Texts.Process1);
            t_create.Start();

            // список временных арматурных стержней
            List<ElementId> elementIdsDel = new List<ElementId>();

            // проверим наличие элементов в проекте
            CheckDataProject();

            SketchPlane sk = doc.ActiveView.SketchPlane;
                // проверим наличие рабочей плоскости - если нет, то зададим                
                if (sk == null)
                {
                    //Transaction work_plane = new Transaction(doc, "Create work plane");
                    //work_plane.Start();
                    Autodesk.Revit.DB.Plane plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.Origin);
                    sk = SketchPlane.Create(doc, plane);
                    doc.ActiveView.SketchPlane = sk;
                    //work_plane.Commit();
                }

                XYZ origin = sk.GetPlane().Origin;
                Plane3D plane3D = new Plane3D(origin, origin + sk.GetPlane().XVec, origin + sk.GetPlane().YVec);

                // СОЗДАНИЕ    ЭСКИЗА  ДЛЯ  ОТДЕЛЬНОГО  СТЕРЖНЯ
                Reference reference = null;
                try
                {
                    // выполнить выбор арматурного стержня          
                    reference = commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element,
                                                                                  new TargetElementSelectionFilter(),
                                                                                  Resourses.Strings.Texts.SelectRebar);
                }
                catch { }

                if (reference == null) { t_create.RollBack(); return Result.Cancelled; }

                Element rebar = doc.GetElement(reference) as Element;
                string name = rebar.GetType().Name;
                

                BoundingBoxXYZ bb = rebar.get_BoundingBox(doc.ActiveView);
                XYZ position_onview = (bb.Max+bb.Min)/2;   // положение элемента на виде 
                position_onview = SketchTools.ProjectPointOnWorkPlane(plane3D, position_onview);
                XYZ distr_path = null;     // направление раскладки
                
                List<Element> Elements = new List<Element>();
                switch (name)
                {
                    case "AreaReinforcement":
                        AreaReinforcementDialog areaDialog = new AreaReinforcementDialog();
                        if (areaDialog.ShowDialog() == DialogResult.Cancel)
                        {
                            t_create.RollBack();  return Result.Cancelled;
                        }

                        m_data.areaDirect = SketchFullApp.areaDirect;
                        m_data.areaLayer = SketchFullApp.areaLayer;

                        // получить плоскости для верхнего и нижнего слоев армирования жб элемента
                        AreaReinforcement areaReinforcement = doc.GetElement(rebar.Id) as AreaReinforcement;
                        if (areaReinforcement == null) break;

                        Element host = doc.GetElement(areaReinforcement.GetHostId());
                        List<Plane3D> plane3Ds = SketchTools.GetPlane3DForLayers(host);
                        if(plane3Ds.Count==0) break;

                        ElementId group_delete = null;
                        Elements = GetRebarInSystemFromArea(areaReinforcement, m_data.areaDirect,m_data.areaLayer, plane3Ds, out distr_path);

                        if (Elements.Count > 20)
                        {
                            DialogResult dr = MessageBox.Show(
                            SketchFull.Resourses.Strings.Texts.Info2, SketchFull.Resourses.Strings.Texts.Attension, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2);
                            if (dr == DialogResult.No) { t_create.RollBack(); return Result.Cancelled; }
                        }
                        Progress progress = new Progress();
                        if (Elements.Count > 20) progress.Show();
                        progress.Text = SketchFull.Resourses.Strings.Texts.Process1;
                        progress.progressBar.Maximum = Elements.Count;
                        
                        int process = 1;

                        vs = null;
                         
                        foreach (Element element in Elements)
                        {
                            group_delete = null;  // тип группы на удаление
                            if (!IsCreateNewSketch(element, out group_delete)) continue;                                                        
                            GroupOnView g1 = CreateSketchDrawing(element, plane3D, group_delete, doc.ActiveView, m_data.Scale);       // создаем новую группу c эскизом стержня                            
                            if (g1 == null) continue;
                            Groups.Add(g1);                             
                            progress.progressBar.Value = process;
                            process++;
                        }

                        if (Elements.Count > 20) progress.Close();
                        // m_data.Aligment = true; // для областей выравнивание обязательно

                        break;
                    case "PathReinforcement":
                        Elements = GetRebarInSystemFromPath(rebar.Id);
                        vs = null;
                        foreach (Element element in Elements)
                        {
                            group_delete = null;
                            if (!IsCreateNewSketch(element, out group_delete)) continue;                           
                            GroupOnView g1 = CreateSketchDrawing(element, plane3D, group_delete,doc.ActiveView, m_data.Scale);       // создаем новую группу c эскизом стержня
                            RebarInSystem rebarInSystem = element as RebarInSystem;                            
                            distr_path = rebarInSystem.GetDistributionPath().Direction;
                            if (g1 == null) continue;
                            Groups.Add(g1);
                        }
                        // m_data.Aligment = true; // для областей выравнивание обязательно
                        break;
                    default:
                        group_delete = null;
                        vs = null;
                        //// создадим дубликат стержня, если он FreeForm
                        //Rebar rebar_free = rebar as Rebar;
                        //        if (rebar_free.IsRebarFreeForm())
                        //        {
                        //            rebar = SketchTools.CreateDublicateRebar(rebar_free, doc.ActiveView);
                        //            elementIdsDel.Add(rebar.Id);
                        //        }


                        if (!IsCreateNewSketch(rebar, out group_delete)) break;  
                        GroupOnView g2 = CreateSketchDrawing(rebar, plane3D, group_delete,doc.ActiveView,m_data.Scale);       // создаем новую группу c эскизом стержня
                        if (g2 == null) break;
                        if (g2.DirMainSegment!=null) distr_path = g2.DirMainSegment.CrossProduct(doc.ActiveView.ViewDirection);
                        Groups.Add(g2);
                        break;
                }

                if(Groups.Count==0)
                {
                    TaskDialog.Show(SketchFull.Resourses.Strings.Texts.Attension, SketchFull.Resourses.Strings.Texts.Info3);
                    t_create.RollBack();
                    return Result.Failed;
                }
                 
                /// <summary>
                /// Точка вставки эскизов
                /// </summary>
                XYZ pos_groups = null;
                try
                {
                    pos_groups = commandData.Application.ActiveUIDocument.Selection.PickPoint(Resourses.Strings.Texts.ShowInsertPoint);
                }
                catch
                {
                    TaskDialog.Show(SketchFull.Resourses.Strings.Texts.Attension,
                        SketchFull.Resourses.Strings.Texts.Info4);
                    t_create.RollBack();
                    return Result.Failed;
                }

                // проверка точки вставки вне зоны черчения
                Outline outline_crop = SketchTools.GetOutlineForCrop(doc.ActiveView, plane3D);
                if (outline_crop != null)
                {
                    if(!outline_crop.Contains(pos_groups,0.001))
                    {
                        MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info5,
                                        SketchFull.Resourses.Strings.Texts.Attension);
                    }
                    
                }
                Categories categories = doc.Settings.Categories;
                Category lineCat = categories.get_Item(BuiltInCategory.OST_Lines);

                if (doc.ActiveView.GetCategoryHidden(lineCat.Id))
                {
                    MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info6, 
                                    SketchFull.Resourses.Strings.Texts.Attension);
                } 
                else
                {                    
                    CategoryNameMap categoryNameMap = lineCat.SubCategories;
                    GraphicsStyle gs = m_data.Line_types[m_data.Line_types_default];
                    foreach (Category cat in categoryNameMap)
                    {
                        if(cat.Name==gs.Name)
                        {
                            if (doc.ActiveView.GetCategoryHidden(cat.Id))
                            {
                                MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info6, 
                                                SketchFull.Resourses.Strings.Texts.Attension);

                            }
                        }

                    }

                }

                // вектор направления расстановки эскизов
                // признак направления по направлению раскладки
                bool IsDistrPath = true;
                vector_groups = GetDirectionSketchs(position_onview, pos_groups, distr_path,out IsDistrPath);
                // направление перпендикулярное расстановке
                XYZ vector_groups_orto = vector_groups.CrossProduct(doc.ActiveView.ViewDirection);
                BoundingBoxXYZ boxXYZ = Groups[0].group.get_BoundingBox(doc.ActiveView);
                
                //Transaction t_groups = new Transaction(doc, "Insert sketch-drawing");
                //t_groups.Start();
                // удалить временный вид
                if (vs!=null) doc.Delete(vs.Id);

                pos_groups = SketchTools.ProjectPointOnWorkPlane(plane3D, pos_groups);

                List<GroupOnView> Sketchs = new List<GroupOnView>();
                // выполним предварительную расстановку групп, если необходимо с выравниванием
                // для зон армирования - выравнивание принудительное
                int i = 0;  // для сдвига на шаг стержней
                            
                vs = null;              

                foreach (GroupOnView g in Groups)
                {                    
                    LocationPoint lG = g.group.Location as LocationPoint;
                    g.insert = SketchTools.ProjectPointOnWorkPlane(plane3D, lG.Point);

                // если не будет выравнивания, то эскиз будет перемещен по вектору на указанное расстояния
                // если будет выравнивание, то вектор и расстояние будут изменены
                // в соответствии с правилами выравнивания    
                // double dist = g.insert.DistanceTo(pos_groups);
                // g.move = (pos_groups - g.insert).Normalize()*(dist + i*g.groupH);
                
                    // затребовано выравнивание
                    if (m_data.Aligment)
                    {

                    //if (g.MainDetailCurve != null)
                    //{
                    //    g.p1 = SketchTools.RoundXYZ(g.MainDetailCurve.GeometryCurve.GetEndPoint(0), 1);
                    //    g.p2 = SketchTools.RoundXYZ(g.MainDetailCurve.GeometryCurve.GetEndPoint(1), 1);
                    //}
                    if (g.MainLineScale != null)
                    {                       
                        g.p1 = SketchTools.RoundXYZ(g.MainLineScale.GetEndPoint(0), 1);
                        g.p2 = SketchTools.RoundXYZ(g.MainLineScale.GetEndPoint(1), 1);
                        g.p1 = SketchTools.ProjectPointOnWorkPlane(plane3D, g.p1);
                        g.p2 = SketchTools.ProjectPointOnWorkPlane(plane3D, g.p2);
                        g.MainLineScale = Line.CreateBound(g.p1, g.p2);
                    }
                    i++;


                    // если стержень не находится на плоскости вида, то выравниваем по точке основного сегмента
                    if (!g.isRebarOnView)
                    {
                        // выполним разворот группы
                        if (g.DirMainSegment != null && g.DirMainDetailCurve != null)  // изменим точку р1
                        {
                            // сегменты в модели и на чертеже должны совпадать
                            double angle = Math.Round(g.DirMainSegment.AngleTo(g.DirMainDetailCurve), 3);
                            if (angle == 3.142) angle = 0;
                            if (angle != 0)
                            {
                                // LocationPoint lp = g.group.Location as LocationPoint;
                                Line axis = Line.CreateUnbound(g.insert, doc.ActiveView.ViewDirection);
                                g.group.Location.Rotate(axis, angle);
                                double angle_check = Math.Round(g.DirMainSegment.AngleTo(g.DirMainDetailCurve), 3);
                                if (angle_check != 0)  // возможно развернули не в ту сторону
                                {
                                    g.group.Location.Rotate(axis, -2 * angle);
                                }
                            }
                        }

                        //g.p1 = SketchTools.RoundXYZ(g.MainDetailCurve.GeometryCurve.GetEndPoint(0),1);
                        //g.p2 = SketchTools.RoundXYZ(g.MainDetailCurve.GeometryCurve.GetEndPoint(1),1);

                        // выполним совмещение со стержнем в модели
                        if (g.project_point != null)
                        {
                            g.project_point = SketchTools.ProjectPointOnWorkPlane(plane3D, g.project_point);
                            XYZ vectorG = g.project_point - g.insert;
                            if (g.MiddleMainDetailCurve != null) vectorG = g.project_point - g.MiddleMainDetailCurve;
                            g.move = g.move + vectorG;
                            g.insert = g.insert + vectorG;
                            g.p1 = SketchTools.RoundXYZ(g.p1 + vectorG, 1);
                            g.p2 = SketchTools.RoundXYZ(g.p2 + vectorG, 1);
                        }
                    }
                    else
                    {
                        // при изменении масштаба - группу предварительно переместить
                        // на середину основного сегмента
                        if (m_data.Scale != 1.0)
                        {
                            g.move = g.MiddleMainDetailCurveScale - g.insert;
                            g.insert = g.MiddleMainDetailCurveScale;
                        }
                    }                            

                    if (IsDistrPath)  // расстановка по направлению раскладки
                        {                        
                            XYZ to_position = (pos_groups - g.insert).Normalize();   // вектор в точку вставки
                            double angle_dir = to_position.AngleTo(vector_groups);   // угол к направлению раскладки
                            double dist = pos_groups.DistanceTo(g.insert) * Math.Cos(angle_dir);
                            g.move = g.move + vector_groups * dist;
                            g.p1 = SketchTools.RoundXYZ(g.p1 + vector_groups * dist,1);
                            g.p2 = SketchTools.RoundXYZ(g.p2 + vector_groups * dist,1);
                            if(g.p1.DistanceTo(g.p2)>0.001)
                            {
                                Line line = Line.CreateBound(g.p1, g.p2);
                                // до настоящего момента - все на одной линии
                                // проверим пересечение с уже размещенными эскизами
                                int cycle_max = 0;
                                foreach (GroupOnView sketch in Sketchs)
                                {
                                    if (cycle_max > 10 * Groups.Count) break;
                                    if (sketch.p1.DistanceTo(sketch.p2) < 0.001) continue;
                                    Line line_current = Line.CreateBound(sketch.p1, sketch.p2);
                                    // DetailCurve newcurve2 = doc.Create.NewDetailCurve(doc.ActiveView, line);                                
                                    if (line_current.Distance(line.GetEndPoint(0)) < 0.1 || line_current.Distance(line.GetEndPoint(1)) < 0.1)
                                    {
                                    // сдвигаем эскиз
                                    g.move = g.move + vector_groups * g.groupH;
                                    g.p1 = SketchTools.RoundXYZ(g.p1 + vector_groups * g.groupH, 1);
                                    g.p2 = SketchTools.RoundXYZ(g.p2 + vector_groups * g.groupH, 1);
                                    line = Line.CreateBound(g.p1, g.p2);
                                    cycle_max++;
                                    }
                                }
                            }
                            Sketchs.Add(g);  // эскиз размещен. Фиксируем этот факт.
                        }
                        else
                        {
                        
                        // расстановка по направлению перпендикулярному раскладке
                        // проведем луч через точку вставки паралелльно раскладке
                        Line raw1 = Line.CreateUnbound(pos_groups, distr_path);
                        Line raw2 = Line.CreateUnbound(g.insert, vector_groups);
                        IntersectionResultArray ira = null;
                        raw1.Intersect(raw2, out ira);
                        if (ira != null)
                        {
                            XYZ cross = ira.get_Item(0).XYZPoint;
                            if (cross != null)
                            {
                                g.finish_dist = g.insert.DistanceTo(cross);
                                g.move = g.move + vector_groups * g.finish_dist;
                                g.insert = cross;
                            }
                        }
                    }
                    }
                    else   // без выравнивания
                    {
                        double dist = g.insert.DistanceTo(pos_groups);
                        g.move = (pos_groups - g.insert).Normalize() * (dist + i * g.groupH);
                    }
                // g.group.Location.Move(g.move);  // Просто перемещаем в указанную точку
            }

            // выполним дополнительное смещения для эскизов на одном уровне
            if (!IsDistrPath)  // расстановка по направлению раскладки
            {
                for (int g = 0; g < Groups.Count; g++)
                {
                    GroupOnView g_current = Groups[g];
                    if (g_current.finish_pos) continue; // эскиз уже на месте
                    int count = Groups.Count(x => SketchTools.CompareXYZ(x.insert, g_current.insert));
                    if (count > 1)  // если эскизов в одном уровне несколько
                    {
                        XYZ last_insert = g_current.insert;
                        BoundingBoxXYZ last_bb = g_current.group.get_BoundingBox(doc.ActiveView);
                        double last_size = last_bb.Max.DistanceTo(last_bb.Min);
                        g_current.finish_pos = true;
                        double sum_dist = last_size / 2;
                        for (int s = 0; s < Groups.Count; s++)
                        {
                            GroupOnView s_current = Groups[s];
                            if (s_current.finish_pos) continue; // эскиз уже на месте

                            if (SketchTools.CompareXYZ(s_current.insert, g_current.insert))
                            {
                                // смещаем текущую группу
                                BoundingBoxXYZ current_bb = s_current.group.get_BoundingBox(doc.ActiveView);
                                double current_size = current_bb.Max.DistanceTo(current_bb.Min);
                                sum_dist = sum_dist + current_size / 2;
                                Groups[s].move = Groups[s].move + vector_groups * sum_dist;                                 
                                s_current.finish_pos = true;
                            }
                        }
                    }
                }
            }
                Progress progressG = new Progress();
                if (Groups.Count > 20) progressG.Show();
                progressG.Text = SketchFull.Resourses.Strings.Texts.Process2;
                progressG.progressBar.Maximum = Groups.Count;
                int processG = 1;

                // перемещения получены - выполняем
                foreach (GroupOnView g in Groups)
                {
                    g.group.Location.Move(g.move);  // перемещаем в указанную точку
                    progressG.progressBar.Value = processG;
                    processG++;
                }

                if (Groups.Count > 20) progressG.Close();
              
                SaveDataToProject();

                // удалить временные арматурные стержни
                doc.Delete(elementIdsDel);

                t_create.Commit();
                return Result.Succeeded;
        }

        /// <summary>
        /// Обновить существующие эскизы
        /// </summary>   
        void UpdateSketchs()
        {
            ICollection<ElementId> group_delete = new List<ElementId>();
            if(RebarsId.Count==0)
            {
                MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info7, SketchFull.Resourses.Strings.Texts.Attension);
                return;
            }
            Progress progress = new Progress();             
            if (RebarsId.Count > 20) progress.Show();
            progress.Text = SketchFull.Resourses.Strings.Texts.Process3;
            progress.progressBar.Maximum = RebarsId.Count;
            int process = 1;

            Transaction t_update = new Transaction(doc, SketchFull.Resourses.Strings.Texts.Process3);
            t_update.Start();

            // проверим наличие элементов в проекте
            CheckDataProject();

            List<int> RebarsIdNew = new List<int>();
            List<int> GroupsIdNew = new List<int>();
            
            for (int e=0;e<RebarsId.Count;e++)
            {
                Element element = doc.GetElement(new ElementId(RebarsId[e]));
                GroupType group = doc.GetElement(new ElementId(GroupsId[e])) as GroupType;
               
                // получим список эскизов текущего типа
                IEnumerable<Element> groups = new FilteredElementCollector(doc).OfClass(typeof(Group)).
                                                  Where(x => x.GetTypeId().IntegerValue == GroupsId[e]);

                if (groups.Count() == 0)
                {
                    if (m_data.Clear)
                    {
                        // больше эскизов нет - удаляем тип
                        group_delete.Add(group.Id);
                    }
                    continue;
                }

                GroupOnView g1 = null;
                if (groups.First() != null)
                {
                    // получим вид на котором создан эскиз
                    Autodesk.Revit.DB.View view = doc.GetElement(groups.First().OwnerViewId) as Autodesk.Revit.DB.View;
                    SketchPlane sk = view.SketchPlane;
                    // проверим наличие рабочей плоскости - если нет, то зададим                
                    if (sk == null)
                    {
                        Autodesk.Revit.DB.Plane plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin);
                        sk = SketchPlane.Create(doc, plane);
                        view.SketchPlane = sk;
                    }

                    XYZ origin = sk.GetPlane().Origin;
                    Plane3D plane3D = new Plane3D(origin, origin + sk.GetPlane().XVec, origin + sk.GetPlane().YVec);
                    SketchCommand.vs = null;
                    g1 = CreateSketchDrawing(element, plane3D, null, view);
                }
                if (g1 != null) // группа получена - выполним замену
                {
                    // t_update.Start();
                    if(vs!=null) doc.Delete(SketchCommand.vs.Id); // удалить временный вид
                    ElementId gt = g1.group.GroupType.Id;
                    foreach (Group gr in groups)
                    {
                        gr.ChangeTypeId(gt);
                    }
                    // исходная копируемая группа не нужна
                    doc.Delete(g1.group.Id);
                    // исходный тип группы не нужен
                    doc.Delete(group.Id);
                    RebarsIdNew.Add(RebarsId[e]);
                    GroupsIdNew.Add(gt.IntegerValue);
                    // t_update.Commit();
                }
                progress.progressBar.Value = process;
                process++;
            }

            progress.Close();

            RebarsId.Clear();
            GroupsId.Clear();
            // запишем новые данные
            foreach (int i in RebarsIdNew) RebarsId.Add(i);
            foreach (int i in GroupsIdNew) GroupsId.Add(i);
            // t_update.Start();
            if(group_delete.Count>0) doc.Delete(group_delete);
            SaveDataToProject();
            t_update.Commit();
        }

        /// <summary>
        /// Обновить существующие эскизы на текущем виде
        /// </summary>   
        void UpdateSketchsOnActiveView()
        {
            if (RebarsId.Count == 0)
            {
                MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info7, SketchFull.Resourses.Strings.Texts.Attension);
                return;
            }
            
            Transaction t_update = new Transaction(doc, SketchFull.Resourses.Strings.Texts.Process4);
            t_update.Start();

            // проверим наличие элементов в проекте
            CheckDataProject();

            List<ElementId> group_delete = new List<ElementId>();
            List<int> RebarsIdNew = new List<int>();
            List<int> GroupsIdNew = new List<int>();

            Progress progress = new Progress();            
            if (RebarsId.Count > 20) progress.Show();
            progress.Text = SketchFull.Resourses.Strings.Texts.Process4;
            progress.progressBar.Maximum = RebarsId.Count;
            int process = 1;

            for (int e = 0; e < RebarsId.Count; e++)
            {
                // информация из базы данных о ранее созданных типах эскизов для стержней
                Element element = doc.GetElement(new ElementId(RebarsId[e]));
                GroupType group = doc.GetElement(new ElementId(GroupsId[e])) as GroupType;

                // получим список эскизов текущего типа для активного вида
                IEnumerable<Element> groups = new FilteredElementCollector(doc).OfClass(typeof(Group)).
                                                  Where(x => x.GetTypeId().IntegerValue == GroupsId[e] &&
                                                        x.OwnerViewId.IntegerValue == doc.ActiveView.Id.IntegerValue);

                // получим список эскизов текущего типа для всего проекта
                IEnumerable<Element> groups_on_other_views = new FilteredElementCollector(doc).OfClass(typeof(Group)).
                                                  Where(x => x.GetTypeId().IntegerValue == GroupsId[e]);

                if (groups.Count()==0) // на текущем виде такого типа эскизов нет. Обновление не выполняем
                {
                    // если эскизов такого типа более нет, то текущий тип удаляем
                    // при условии, что его нет и на других видах
                    if (m_data.Clear && groups_on_other_views.Count()==0)
                    {                        
                        group_delete.Add(group.Id);                        
                    }
                    continue;   // нет эскизов для обновления. Берем следующий
                }

                GroupOnView g1 = null;
                if (groups.First() != null)
                {   
                    // если на виде есть несколько эскизов данного типа 
                    // то берем первый. Создаем его. Для остальных экземпляров просто меняем тип
                    // таким образом местоположение остается старым
                    SketchPlane sk = doc.ActiveView.SketchPlane;
                    // проверим наличие рабочей плоскости - если нет, то зададим                
                    if (sk == null)
                    {
                        Autodesk.Revit.DB.Plane plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.Origin);
                        sk = SketchPlane.Create(doc, plane);
                        doc.ActiveView.SketchPlane = sk;
                    }

                    XYZ origin = sk.GetPlane().Origin;
                    Plane3D plane3D = new Plane3D(origin, origin + sk.GetPlane().XVec, origin + sk.GetPlane().YVec);
                    // обязательно создаем новый тип группы и новую временную группу
                    // в последующем: для всех экземпляров группы выполним замену на новый тип
                    // временную группу удалим. Старый тип удалим при условии, что его нет на других видах
                    SketchCommand.vs = null;
                    g1 = CreateSketchDrawing(element, plane3D, null, doc.ActiveView);
                }
                if (g1 != null) // временная группа получена - выполним замену типа для всех эскизов текущего вида
                {
                    //t_update.Start();
                    if(vs!=null) doc.Delete(SketchCommand.vs.Id); // удалить временный вид
                    ElementId gt = g1.group.GroupType.Id;
                    foreach (Group gr in groups)
                    {
                        gr.ChangeTypeId(gt);
                    }
                    // созданная временная группа не более нужна                    
                    doc.Delete(g1.group.Id);
                    // исходный тип группы более не нужен
                    // при условии, что его нет на других видах
                    if(groups_on_other_views.Count() == 0) doc.Delete(group.Id);
                    RebarsIdNew.Add(RebarsId[e]);
                    GroupsIdNew.Add(gt.IntegerValue);

                    RebarsId.RemoveAt(e);
                    GroupsId.RemoveAt(e);
                    e--;
                    // t_update.Commit();
                }
                progress.progressBar.Value = process;
                process++;
            }

            progress.Close();
                      
            // запишем новые данные
            foreach (int i in RebarsIdNew) RebarsId.Add(i);
            foreach (int i in GroupsIdNew) GroupsId.Add(i);
            // t_update.Start();
            if (group_delete.Count > 0) doc.Delete(group_delete);
            SaveDataToProject();
            t_update.Commit();
        }


        /// <summary>
        /// Создавать ли новый тип эскиза ?
        /// </summary>         
        bool IsCreateNewSketch(Element rebar, out ElementId groupId_delete)
        {
            //Transaction t_groups = new Transaction(doc, "Prepared sketch-drawing");
            //t_groups.Start();
            groupId_delete = null;
            for (int e=0;e<RebarsId.Count;e++)
            {
                bool Is_Sketch_on_other_view = false;
                if(RebarsId[e]==rebar.Id.IntegerValue) // это текущий стержень
                {
                    int group_type_Id = GroupsId[e];   // это текущий тип эскиза
                    // проверим число эскизов, созданных на текущем виде. Должен быть один.
                    // Если их больше: удаляем все и со всех видов и создаем один новый
                    // Или ничего не делаем
                    IEnumerable<Element> groups = new FilteredElementCollector(doc).OfClass(typeof(Group)).
                                                  Where(x => x.GetTypeId().IntegerValue == group_type_Id &&
                                                  x.OwnerViewId.IntegerValue == doc.ActiveView.Id.IntegerValue);
                    IEnumerable<Element> groups_on_other_view = new FilteredElementCollector(doc).OfClass(typeof(Group)).
                             Where(x => x.GetTypeId().IntegerValue == group_type_Id &&
                                   x.OwnerViewId.IntegerValue != doc.ActiveView.Id.IntegerValue);
                    if (groups.Count() == 1)
                    {
                        if (groups_on_other_view.Count() > 0)
                        {
                            if (MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info8,
                                SketchFull.Resourses.Strings.Texts.Attension, MessageBoxButtons.YesNo) ==
                             DialogResult.Yes)
                            {
                                // t_groups.Commit();
                                return true;
                            }
                            else
                            {
                                // t_groups.Commit();
                                return false;
                            }
                        }
                        // удаляем инфо из базы. Удаляем тип группы
                        RebarsId.RemoveAt(e);
                        GroupsId.RemoveAt(e);
                        groupId_delete = new ElementId(group_type_Id);
                        e--;
                        // doc.Delete(new ElementId(group_type_Id));
                        // t_groups.Commit();
                        return true;
                    }
                    if (groups.Count() > 1)
                    {
                        if (MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info9,
                            SketchFull.Resourses.Strings.Texts.Attension, MessageBoxButtons.YesNo) ==
                             DialogResult.Yes)
                        {
                            // удаляем инфо из базы. Удаляем тип группы
                            RebarsId.RemoveAt(e);
                            GroupsId.RemoveAt(e);
                            groupId_delete = new ElementId(group_type_Id);
                            e--;
                            // doc.Delete(new ElementId(group_type_Id));
                            return true;
                        }
                        else return false;
                    }
                    if(groups.Count()==0 && groups_on_other_view.Count() > 0)
                    {
                        Is_Sketch_on_other_view = true;
                    }
                }

                if(Is_Sketch_on_other_view)
                {
                    if (MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info8,
                                        SketchFull.Resourses.Strings.Texts.Attension, MessageBoxButtons.YesNo) ==
                            DialogResult.Yes) return true;
                    else return false;
                }

            }
            // t_groups.Commit();
            return true;
        }

        ///// <summary>
        ///// Удалить существующую группу для стержня на текущем виде
        ///// </summary>         
        //void DeleteExistGroup(Element rebar)
        //{
        //    Transaction t_groups = new Transaction(doc, "Prepared sketch-drawing");
        //    t_groups.Start();

        //    for (int r = 0; r < RebarsId.Count; r++)
        //    {
        //        if (RebarsId[r] == rebar.Id.IntegerValue)
        //        {
        //            Group group = doc.GetElement(new ElementId(GroupsId[r])) as Group; // получим соответствующую группу
        //            if (group != null)
        //            {
        //                if (group.OwnerViewId.IntegerValue == doc.ActiveView.Id.IntegerValue)
        //                {
        //                    // группа будет создана заново. 
        //                    // Тип группы удаляем
        //                    ElementId gt = group.GetTypeId();                           
        //                    // удаляем имеющиеся копии и тип группы
        //                    IEnumerable<Element> groups = new FilteredElementCollector(doc).OfClass(typeof(Group)).Where(x => x.GetTypeId().IntegerValue == gt.IntegerValue);
        //                    if (groups.Count() > 1)
        //                    {
        //                        if (MessageBox.Show("Обнаружено несколько копий эскизов для данного стержня. Эти копии останутся без изменений. Удалить копии ?", "Предупреждение", MessageBoxButtons.YesNo) ==
        //                             DialogResult.Yes)
        //                        {
        //                            doc.Delete(gt);
        //                        }
        //                        else
        //                        {
        //                            doc.Delete(new ElementId(GroupsId[r])); // Существующую группу удаляем.
        //                        }

        //                    }
        //                    else
        //                    {
        //                        doc.Delete(gt);  // вместе с типом будет удалена группа
        //                    }
        //                    // запись удаляем. Информация о новой паре будет добавлена при сохранении
        //                    RebarsId.RemoveAt(r);
        //                    GroupsId.RemoveAt(r);
        //                    t_groups.Commit();
        //                    return;
        //                }                       
        //            }
        //            else
        //            {
        //                // такой группы уже нет - запись удаляем
        //                RebarsId.RemoveAt(r);
        //                GroupsId.RemoveAt(r);
        //                r--;
        //            }
        //        }
        //    }
        //    t_groups.Commit();
        //    return;
        //}

        /// <summary>
        /// Записать данные в проект
        /// </summary>
        void SaveDataToProject()
        {
            // получить схему хранения данных
            schema_sketchs = Schema.Lookup(SchemaSketchs);
            if (null == schema_sketchs) PreparedSchemaSketchs();

            // проверяем наличие ранее созданного вида набора данных
            FilteredElementCollector collectorV = new FilteredElementCollector(doc);
            collectorV.WherePasses(new ElementClassFilter(typeof(DataStorage)));
            var storage = from element in collectorV where element.Name == "SketchFull" select element;
            if (storage.Count() == 0)
            {
                ds = DataStorage.Create(doc);
                ds.Name = "SketchFull";                
            }
            else ds = storage.First() as DataStorage;

            // получить данные по схеме хранения
            Entity ent_storage = new Entity(schema_sketchs);
            if (ent_storage == null) return;
            if (ent_storage.Schema != null)
            {    
                // добавить необходимые пары значений: номер стержня и номер ТИПА группы
                // на одном виде для стержня может быть создан один эскиз
                foreach (GroupOnView gov in Groups)
                {
                    // добавим новую пару
                    RebarsId.Add(gov.element.Id.IntegerValue);
                    GroupsId.Add(gov.group.GetTypeId().IntegerValue);                    
                }
                // запишем данные для сохранения
                Field
                field_current = schema_sketchs.GetField("RebarId");
                ent_storage.Set<IList<int>>(field_current, RebarsId);
                field_current = schema_sketchs.GetField("GroupId");
                ent_storage.Set<IList<int>>(field_current, GroupsId);
                field_current = schema_sketchs.GetField("IsUsePrefix");
                ent_storage.Set<bool>(field_current, m_data.UsePrefix);

                if (m_data.Tags.Count > 0)
                {
                    field_current = schema_sketchs.GetField("TagId");
                    ent_storage.Set<int>(field_current, m_data.Tags[m_data.Tag_default].Id.IntegerValue);
                }
                if (m_data.Fonts.Count > 0)
                {
                    field_current = schema_sketchs.GetField("FontId");
                    ent_storage.Set<int>(field_current, m_data.Fonts[m_data.Font_default].Id.IntegerValue);
                }
                if (m_data.Line_types.Count > 0)
                {
                    field_current = schema_sketchs.GetField("LineId");
                    ent_storage.Set<int>(field_current, m_data.Line_types[m_data.Line_types_default].Id.IntegerValue);
                }
                field_current = schema_sketchs.GetField("IsAligment");
                ent_storage.Set<bool>(field_current, m_data.Aligment);
                field_current = schema_sketchs.GetField("IsClear");
                ent_storage.Set<bool>(field_current, m_data.Clear);

                field_current = schema_sketchs.GetField("IsTotalLength");
                ent_storage.Set<bool>(field_current, m_data.IsTotalLength);

                field_current = schema_sketchs.GetField("Scale");
                ent_storage.Set<double>(field_current, m_data.Scale, UnitTypeId.Custom);

                ds.SetEntity(ent_storage);
            }
        }

        /// <summary>
        /// Подготовить шаблон схемы для записи общих данных проекта
        /// </summary>
        void PreparedSchemaSketchs()
        {
            SchemaBuilder schemaBuilder = new SchemaBuilder(SchemaSketchs);  // создать схему данных

            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
            schemaBuilder.SetSchemaName("SketchsOnViews");

            FieldBuilder
                fieldBuilder = schemaBuilder.AddArrayField("RebarId", typeof(int));
                fieldBuilder.SetDocumentation("Id арматуры");
                fieldBuilder = schemaBuilder.AddArrayField("GroupId", typeof(int));
                fieldBuilder.SetDocumentation("Id типа группы");                 
                fieldBuilder = schemaBuilder.AddSimpleField("IsUsePrefix", typeof(bool));
                fieldBuilder.SetDocumentation("Признак использования марки эскиза");
                fieldBuilder = schemaBuilder.AddSimpleField("TagId", typeof(int));
                fieldBuilder.SetDocumentation("Id марки для префикса");
                fieldBuilder = schemaBuilder.AddSimpleField("FontId", typeof(int));
                fieldBuilder.SetDocumentation("Id шрифта");
                fieldBuilder = schemaBuilder.AddSimpleField("LineId", typeof(int));
                fieldBuilder.SetDocumentation("Id линии");
                fieldBuilder = schemaBuilder.AddSimpleField("IsAligment", typeof(bool));
                fieldBuilder.SetDocumentation("Признак выравнивания");

                fieldBuilder = schemaBuilder.AddSimpleField("IsClear", typeof(bool));
                fieldBuilder.SetDocumentation("Признак удаления эскизов при обновлении");

                fieldBuilder = schemaBuilder.AddSimpleField("IsTotalLength", typeof(bool));
                fieldBuilder.SetDocumentation("Признак добавления диаметра стержня к марке");

                fieldBuilder = schemaBuilder.AddSimpleField("Scale", typeof(double));
                fieldBuilder.SetSpec(SpecTypeId.Custom);
                fieldBuilder.SetDocumentation("Масштаб эскиза");

                schema_sketchs = schemaBuilder.Finish();
        }
        /// <summary>
        /// Проверить данные проекта
        /// </summary>
        void CheckDataProject()
        {
            ICollection<ElementId> group_delete = new List<ElementId>();
            // проверим наличие элементов в модели
            for (int r = 0; r < RebarsId.Count; r++)
            {
                Element element = doc.GetElement(new ElementId(RebarsId[r]));
                if (element == null)
                {
                    Element group_check = doc.GetElement(new ElementId(GroupsId[r]));
                    if (group_check != null)
                    {
                        if(m_data.Clear) group_delete.Add(group_check.Id);
                    }
                    RebarsId.RemoveAt(r);
                    GroupsId.RemoveAt(r);
                    r--;
                    continue;
                }
                Element group = doc.GetElement(new ElementId(GroupsId[r]));
                if (group == null)
                {
                    RebarsId.RemoveAt(r);
                    GroupsId.RemoveAt(r);
                    r--;
                    continue;
                }
            }
            if (group_delete.Count > 0)
            {
                // Transaction t = new Transaction(doc, "Check sketch-drawing");
                // t.Start();
                doc.Delete(group_delete);
                // t.Commit();
            }
        }
            /// <summary>
            /// Прочитать данные проекта
            /// </summary>
        void ReadDataFromProject()
        {
            // получить схему хранения данных
            schema_sketchs = Schema.Lookup(SchemaSketchs);
            if (null == schema_sketchs) return;  // нет схемы

            // проверяем наличие ранее созданного вида набора данных
            FilteredElementCollector collectorV = new FilteredElementCollector(doc);
            collectorV.WherePasses(new ElementClassFilter(typeof(DataStorage)));
            var storage = from element in collectorV where element.Name == "SketchFull" select element;
            if (storage.Count() == 0) return; // нет набора данных

            ds = storage.First() as DataStorage;  
            // получить данные по схеме хранения
            Entity ent_storage = ds.GetEntity(schema_sketchs);
            if (ent_storage == null) return;   // не удалось получить доступ к хранилищу
            

            if (ent_storage.Schema != null)    // читаем данные из хранилища
            {                 
                m_data.UsePrefix = ent_storage.Get<bool>("IsUsePrefix");

                int TagId = ent_storage.Get<int>("TagId");
                int index = m_data.Tags.FindIndex(x => x.Id.IntegerValue == TagId);
                if (index > -1) m_data.Tag_default = index;

                int FontId = ent_storage.Get<int>("FontId");
                index = m_data.Fonts.FindIndex(x => x.Id.IntegerValue == FontId);
                if (index > -1) m_data.Font_default = index;


                int LineId = ent_storage.Get<int>("LineId");
                index = m_data.Line_types.FindIndex(x => x.Id.IntegerValue == LineId);
                if (index > -1) m_data.Line_types_default = index;

                m_data.Aligment = ent_storage.Get<bool>("IsAligment");
                m_data.Clear = ent_storage.Get<bool>("IsClear");
                m_data.IsTotalLength = ent_storage.Get<bool>("IsTotalLength");
                m_data.Scale = ent_storage.Get<double>("Scale",UnitTypeId.Custom);
                if (m_data.Scale <= 0) m_data.Scale = 1;
                RebarsId = ent_storage.Get<IList<int>>("RebarId").ToList();
                GroupsId = ent_storage.Get<IList<int>>("GroupId").ToList();
            }           

            return;
        }
        /// <summary>
        /// Получить список стержней в системе для армирования по траектории
        /// </summary>
        List<Element> GetRebarInSystemFromArea(AreaReinforcement areaReinforcement, AreaDirect areaDirect, AreaLayer areaLayer, List<Plane3D> plane3Ds, out XYZ distr_path)
        {
            List<double> L = new List<double>();   // список длин стержней
            distr_path = XYZ.Zero;
            XYZ dir = areaReinforcement.Direction; // это основное направление зоны армирования
            // bool select_dir = areaDirect == AreaDirect.Main ? true : false;
            IList<ElementId> elementIds = areaReinforcement.GetRebarInSystemIds();
            List<Element> RIS = new List<Element>();
            foreach (ElementId eid in elementIds)
            {
                RebarInSystem ris = doc.GetElement(eid) as RebarInSystem;
                XYZ normal = ris.Normal;
                // проверим соответствие направлению
                switch (areaDirect)
                {
                    case AreaDirect.Main:
                        if (Math.Abs(Math.Round(Math.Cos(dir.AngleTo(normal)), 3)) == 0) break;
                        continue;                        
                    default:
                        if (Math.Abs(Math.Round( Math.Cos(dir.AngleTo(normal)),3))==1) break;                         
                        continue;
                }                
                    // проверим соответствие слою армирования
                    AreaLayer current_area_layer = SketchTools.GetLayerForRebarInSystem(ris, plane3Ds);
                    if (current_area_layer == areaLayer)
                    {
                        double length = Math.Round(ris.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(),3);
                        if (L.Find(x => x == length) == 0)
                        {
                            RIS.Add(doc.GetElement(eid));
                            distr_path = normal;
                            L.Add(length);
                        }
                    }
                }
            return RIS;
        }

        /// <summary>
        /// Получить список стержней в системе для армирования по траектории
        /// </summary>
        List<Element> GetRebarInSystemFromPath(ElementId pr)
        {
            PathReinforcement pathReinforcement = doc.GetElement(pr) as PathReinforcement;
            IList<ElementId> elementIds = pathReinforcement.GetRebarInSystemIds();
            List<Element> RIS = new List<Element>();             
            foreach (ElementId eid in elementIds)
            {
                RIS.Add(doc.GetElement(eid));
            }
            return RIS;
        }

        ///// <summary>
        ///// Получить минимальную высоту прямоугольной зоны по заданному направлению
        ///// </summary>
        //double GetHZoneByBox(BoundingBoxXYZ boxXYZ, Plane3D plane, XYZ v1)
        //{
        //    XYZ p1 = SketchTools.ProjectPointOnWorkPlane(plane, boxXYZ.Max);
        //    XYZ p2 = SketchTools.ProjectPointOnWorkPlane(plane, boxXYZ.Min);
        //    XYZ v2 = doc.ActiveView.ViewDirection.CrossProduct(v1);

        //    Line raw1 = Line.CreateUnbound(p1, v1);           
        //    Line raw4 = Line.CreateUnbound(p2, v2);

        //    IntersectionResultArray ira = null;
        //    raw4.Intersect(raw1, out ira);
        //    if (ira == null) return 0;
        //    XYZ p3 = ira.get_Item(0).XYZPoint;
            
        //    return Math.Min(p1.DistanceTo(p3), p2.DistanceTo(p3));
        //}

        //    /// <summary>
        //    /// Получить прямоугольную зону
        //    /// </summary>
        //    Outline GetRectZoneByBox(BoundingBoxXYZ boxXYZ, Plane3D plane, XYZ v1, XYZ v2)
        //{
            
        //    XYZ p1 = SketchTools.ProjectPointOnWorkPlane(plane, boxXYZ.Max);
        //    XYZ p2 = SketchTools.ProjectPointOnWorkPlane(plane, boxXYZ.Min);

        //    Line raw1 = Line.CreateUnbound(p1, v1);
        //    Line raw2 = Line.CreateUnbound(p1, v2);
        //    Line raw3 = Line.CreateUnbound(p2, v1);
        //    Line raw4 = Line.CreateUnbound(p2, v2);

        //    IntersectionResultArray ira = null;
        //    raw4.Intersect(raw1,out ira);
        //    if (ira == null) return null;
        //    XYZ p3 = ira.get_Item(0).XYZPoint;
            
        //    ira = null;
        //    raw2.Intersect(raw3, out ira);
        //    if (ira == null) return null;
        //    XYZ p4 = ira.get_Item(0).XYZPoint;

        //    Outline outline = new Outline(p1, p2);
        //    outline.AddPoint(p3); outline.AddPoint(p4);           
        //    return outline;
        //}


        //private void RotateBoundingBox(View3D view3d)
        //{
        //    if (!view3d.IsSectionBoxActive)
        //    {
        //        TaskDialog.Show("Revit", "The section box for View3D isn't active.");
        //        return;
        //    }
        //    BoundingBoxXYZ box = view3d.GetSectionBox();
        //    XYZ p1= box.Transform.OfPoint(box.Max);
        //    // Create a rotation transform to apply to the section box 
        //    XYZ origin = new XYZ(0, 0, 0);
        //    XYZ axis = new XYZ(0, 0, 1);

        //    // Rotate 30 degrees
        //    Transform rotate = Transform.CreateRotationAtPoint(axis, Math.PI / 6.0, origin);

        //    // Transform the View3D's section box with the rotation transform
        //    box.Transform = box.Transform.Multiply(rotate);
        //    doc.Regenerate();

        //    XYZ p2 = box.Transform.OfPoint(box.Max);

        //    // Set the section box back to the view (requires an open transaction)
        //    view3d.SetSectionBox(box);
        //    box = view3d.GetSectionBox();
        //}


        ///// <summary>
        ///// Проверить положение стержня на текущем виде
        ///// </summary>
        //bool CheckRebarPositionOnView(Element element, SketchDirect direct)
        //{           

             

        //    //if (doc.ActiveView.CropBoxActive)
        //    //{                
        //    //    // система координат бокса - локальная
        //    //    BoundingBoxXYZ boundingBoxView = doc.ActiveView.CropBox;
        //    //    // предельные точки бокса - в глобальной системе координат
        //    //    XYZ pMax = boundingBoxView.Transform.OfPoint(boundingBoxView.Max);
        //    //    XYZ pMin = boundingBoxView.Transform.OfPoint(boundingBoxView.Min);
        //    //    pMax = new XYZ(pMax.X, pMax.Y, 9.84);
        //    //    pMin = new XYZ(pMin.X, pMin.Y, 0.0);
        //    //    Outline border = new Outline(pMin, pMax);
        //    //    // получим арматуру в пределах данной рамки
        //    //    BoundingBoxIsInsideFilter insideFilter =
        //    //    new BoundingBoxIsInsideFilter(border);
        //    //    FilteredElementCollector collector = new FilteredElementCollector(doc);
        //    //    IList<Element> notContainFounds =
        //    //        collector.OfClass(typeof(Rebar)).WherePasses(insideFilter).ToElements();

        //    //    //Transform t_view = Transform.Identity;

        //    //    //t_view.BasisX = doc.ActiveView.RightDirection;
        //    //    //t_view.BasisY = doc.ActiveView.UpDirection;
        //    //    //t_view.BasisZ = doc.ActiveView.ViewDirection;

        //    //    //BoundingBoxXYZ boundingBoxElement = element.get_BoundingBox(doc.ActiveView);
        //    //    //XYZ p1 = boundingBoxView.Transform.Inverse.OfPoint(boundingBoxElement.Max);
        //    //    //XYZ p2 = boundingBoxView.Transform.OfPoint(boundingBoxElement.Max);

        //    //    ////boundingBoxElement.Transform = boundingBoxElement.Transform.Multiply(t_view);

        //    //    //Outline out_view = new Outline(new XYZ(boundingBoxView.Max.X, boundingBoxView.Max.Y, 0),
        //    //    //                   new XYZ(boundingBoxView.Min.X, boundingBoxView.Min.Y, 0));
        //    //    //Outline out_el = new Outline(new XYZ(boundingBoxElement.Max.X, boundingBoxElement.Max.Y, 0),
        //    //    //                             new XYZ(boundingBoxElement.Min.X, boundingBoxElement.Min.Y, 0));
                
        //    //    //if (out_view.Intersects(out_el, 0.01)) return false;
        //    //    //if (!out_view.Contains(out_el.MaximumPoint, 0.01)) return false;
        //    //    //if (!out_view.Contains(out_el.MinimumPoint, 0.01)) return false;
        //    //}             

        //    FamilySymbol fs = null;
        //    if (m_data.Tags.Count > 0) fs = m_data.Tags[m_data.Tag_default];
        //    if (!m_data.UsePrefix) fs = null;

        //    BuildSketchRebar bsr = new BuildSketchRebar(element,null,                     
        //            m_data.Line_types[m_data.Line_types_default + 1],
        //            m_data.Fonts[m_data.Font_default],
        //            fs);

        //    bsr.GetRebarsCurves();
        //    if (!bsr.IsRebarOnViewPlane && bsr.rebarIn==null) return false;
        //    if (Math.Round(bsr.DirMainSegment.AngleTo(vector_groups),3)== Math.Round(Math.PI/2,3)) return true;
        //    return false;
        //}

        /// <summary>
        /// Получить направление расстановки эскизов
        /// </summary>
        XYZ GetDirectionSketchs(XYZ pos_rebar, XYZ pos_groups, XYZ direct_path, out bool IsDistrPath)
        {
            IsDistrPath = true;
            XYZ dir = (pos_groups - pos_rebar).Normalize();
            if (direct_path == null) return dir;
            double angle = direct_path.AngleTo(dir);
            if (angle < Math.PI / 4) return direct_path;
            if (angle > 3* Math.PI / 4) return direct_path.Negate();
            direct_path = direct_path.CrossProduct(doc.ActiveView.ViewDirection);
            angle = direct_path.AngleTo(dir);
            IsDistrPath = false;
            if (angle <= Math.PI / 2) return direct_path;
            return direct_path.Negate();
        }
        //    XYZ GetDirectionByDialog(SketchDirect direct)
        //{
        //    if (direct == SketchDirect.Down) return doc.ActiveView.UpDirection.Negate();
        //    if (direct == SketchDirect.Up) return doc.ActiveView.UpDirection;
        //    if (direct == SketchDirect.Right) return doc.ActiveView.RightDirection;
        //    return doc.ActiveView.RightDirection.Negate();
        //}

        /// <summary>
        /// Создать группу и вернуть ее и параметры для расстановки
        /// </summary>
        GroupOnView CreateSketchDrawing(Element rebar,Plane3D plane3D, ElementId group_delete, Autodesk.Revit.DB.View active_view, double scale=1.0)
        {
           
            GroupOnView gov = new GroupOnView();             

            //Transaction t = new Transaction(doc, "Create sketch-drawing");
            //t.Start();

            if (group_delete != null) doc.Delete(group_delete);            
                
            FamilySymbol fs = null;
            if (m_data.Tags.Count > 0) fs = m_data.Tags[m_data.Tag_default];
            if (!m_data.UsePrefix) fs = null;

            // Параметры построения чертежа            
            BuildSketchRebar buildImage = new BuildSketchRebar(rebar,plane3D,                     
                    m_data.Line_types[m_data.Line_types_default],
                    m_data.Fonts[m_data.Font_default],
                    fs);
            buildImage.active_view = active_view;
            buildImage.to_current_view.BasisX = active_view.RightDirection;
            buildImage.to_current_view.BasisY = active_view.UpDirection;
            buildImage.to_current_view.BasisZ = active_view.ViewDirection;
            buildImage.scale = scale;
            buildImage.IsTotalLength = m_data.IsTotalLength;

            // buildImage.coeff_font = 0.25;           

            if (!buildImage.PreparedDataSegements())
            {                
                //t.RollBack();
                return null;
            }
            
            if (buildImage.Eids.Count == 0)
            {                
                //t.RollBack();
                return null;
            }

            gov.element = rebar;
            gov.isRebarOnView = buildImage.IsRebarOnViewPlane;
            gov.project_point = buildImage.ProjectMainSegmentOnView;
            gov.DirMainSegment = buildImage.DirMainSegment;
            // gov.MainDetailCurve = buildImage.MainDetailCurve;
            gov.MainLine = buildImage.MainLine;
            gov.MainLineScale = buildImage.MainLineScale;


            //buildImage.transaction = t;
            //buildImage.bending = true;
            // buildImage.hooks_length = false;

            //t.Commit();
            //t.Start();

            // создаем основную группу
            gov.group = doc.Create.NewGroup(buildImage.Eids);           
            gov.group.GroupType.Name = "Sketch_rebar_" + rebar.Id.IntegerValue.ToString() + "_" + doc.ActiveView.Name;            
            gov.groupH = CalculateGroupH(gov.group.get_BoundingBox(doc.ActiveView), plane3D);
            
            // t.Commit();             
            return gov;
        }


        /// <summary>
        /// Рассчитать высоту группы на чертеже
        /// </summary>
        double CalculateGroupH(BoundingBoxXYZ boxXYZ, Plane3D plane)
        {
            if (boxXYZ == null) return 0;
            XYZ p1 = SketchTools.ProjectPointOnWorkPlane(plane, boxXYZ.Max);
            XYZ p2 = SketchTools.ProjectPointOnWorkPlane(plane, boxXYZ.Min);
            XYZ vector = (p2 - p1).Normalize();
            double angle = vector.AngleTo(doc.ActiveView.RightDirection);
            if (angle > Math.PI / 2) angle = Math.PI - angle;
            double H1= p1.DistanceTo(p2) * Math.Sin(angle);
            double H2 = p1.DistanceTo(p2) * Math.Cos(angle);
            return Math.Min(H1, H2);
        }

        ///// <summary>
        ///// Создание временного вида для подготовки эскизов
        ///// </summary>
        //ViewDrafting CreateTempView()
        //{
        //    try
        //    {
        //        ViewFamilyType viewFamilyType = null;
        //        FilteredElementCollector collector = new FilteredElementCollector(doc);
        //        var viewFamilyTypes = collector.OfClass(typeof(ViewFamilyType)).ToElements();
        //        foreach (Element e in viewFamilyTypes)
        //        {
        //            ViewFamilyType v = e as ViewFamilyType;
        //            if (v.ViewFamily == ViewFamily.Drafting)
        //            {
        //                viewFamilyType = v;
        //                break;
        //            }
        //        }
        //        ViewDrafting drafting = ViewDrafting.Create(doc, viewFamilyType.Id);
        //        if (null == drafting)
        //        {
        //            return null;
        //        }
        //        return drafting;
        //    }
        //    catch
        //    {
        //        return null;
        //    }            
        //}

        //XYZ FindRightDirection(XYZ viewDirection)
        //{
        //    // Because this example only allow the beam to be horizontal,
        //    // the created viewSection should be vertical, 
        //    // the same thing can also be found when the user select wall or floor.
        //    // So only need to turn 90 degree around Z axes will get Right Direction.  

        //    double x = -viewDirection.Y;
        //    double y = viewDirection.X;
        //    double z = viewDirection.Z;
        //    XYZ direction = new XYZ(x, y, z);
        //    return direction;
        //}
        ///// <summary>
        ///// Формирование данных по участкам армирования
        ///// </summary>
        //void DataBySegments(Rebar rebar)
        //{
        //    RebarShape rs = doc.GetElement(rebar.GetShapeId()) as RebarShape;
        //    RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
        //    RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
        //    ParameterSet pset = rebar.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 

        //    // Цикл по сегментам в данной форме rsds.NumberOfSegments
        //    for (int i = 0; i < rsds.NumberOfSegments; i++)
        //    {
        //        TextOnRebar tor = new TextOnRebar();                                      // создаем будущую надпись над сегментом
        //        tor.rebar = rebar;                                                 // запишем текущий стержень
        //        RebarShapeSegment segment = rsds.GetSegment(i);                           // определяем сегмент

        //        IList<RebarShapeConstraint> ILrsc = segment.GetConstraints();             // параметры сегмента                

        //        foreach (RebarShapeConstraint rsc in ILrsc)                               // разбираем каждый сегмент в отдельности
        //        {
        //            // получим длину сегмента
        //            RebarShapeConstraintSegmentLength l = rsc as RebarShapeConstraintSegmentLength;

        //            if (l != null)
        //            {
        //                ElementId pid = l.GetParamId();
        //                Element elem = doc.GetElement(pid);
        //                foreach (Parameter pr in pset)
        //                {
        //                    if (pr.Definition.Name == elem.Name)
        //                    {
        //                        tor.guid = pr.GUID;
        //                        // с учетом локальных особенностей
        //                        tor.value_initial = rebar.get_Parameter(pr.Definition).AsDouble();
        //                        // tor.value = tor.value_initial + GetAddValue(template, tor.guid, rebar as Rebar);
        //                        if (tor.value <= 0) tor.value = tor.value_initial;
        //                        if (tor.value > 0) break;
        //                    }
        //                }

        //                // запишем для контроля имя параметра
        //                tor.name = elem.Name;                                                                          // добавим метку
        //                                                                                                               // определяем, что данный сегмент является дугой
        //                try
        //                {
        //                    RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeDefaultBend");
        //                    tor.arc = true;
        //                }
        //                catch { }
        //                try
        //                {
        //                    RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendRadius");
        //                    tor.arc = true;
        //                }
        //                catch { }

        //                try
        //                {
        //                    RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendArcLength");
        //                    tor.arc = true;
        //                }
        //                catch { }
        //                continue;
        //            }

        //            // определяем, что данный участок наклонный
        //            RebarShapeConstraintProjectedSegmentLength proj = rsc as RebarShapeConstraintProjectedSegmentLength;
        //            // работаем с вертикальной проекцией текущего участка
        //            if (proj != null && tor.guidV.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.V), 0) == 1)
        //            {
        //                tor.incline = InclineText.Incline;
        //                ElementId pid = proj.GetParamId();
        //                Element elem = doc.GetElement(pid);
        //                foreach (Parameter pr in pset)
        //                {
        //                    if (pr.Definition.Name == elem.Name)
        //                    {
        //                        tor.guidV = pr.GUID;
        //                        tor.valueV = rebar.get_Parameter(pr.Definition).AsDouble();
        //                        if (tor.valueV > 0) break;
        //                    }
        //                }
        //                tor.nameV = elem.Name;                                                                          // добавим метку                       
        //                continue;

        //            }

        //            if (proj != null && tor.guidH.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.U), 0) == 1)
        //            {
        //                tor.incline = InclineText.Incline;
        //                ElementId pid = proj.GetParamId();
        //                Element elem = doc.GetElement(pid);

        //                foreach (Parameter pr in pset)
        //                {
        //                    if (pr.Definition.Name == elem.Name)
        //                    {
        //                        tor.guidH = pr.GUID;
        //                        tor.valueH = rebar.get_Parameter(pr.Definition).AsDouble();
        //                        if (tor.valueH > 0) break;

        //                    }
        //                }
        //                tor.nameH = elem.Name;                                                                          // добавим метку

        //                continue;
        //            }
        //        }

        //        // если проекция наклонного участока практически равна базовому участку, то наклонный не показываем
        //        if (tor.value_str == tor.valueH_str) tor.valueH = 0;
        //        if (tor.value_str == tor.valueV_str) tor.valueV = 0;
        //        lg.Add(tor);   // внесем сегмент в общий список
        //    }
        //}

        /// <summary>
        /// Получить данные по аннотациям для арматуры
        /// </summary> 
        void GetRebarTags()
        {
            // общий список аннотаций Tags             
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            FilteredElementIterator List_tags = collector2.WherePasses(new ElementClassFilter(typeof(FamilySymbol))).GetElementIterator();    // .OfType<Element>().ToList();
            List_tags.Reset();
            while (List_tags.MoveNext())
            {
                //добавим марку к списку 
                FamilySymbol tn = List_tags.Current as FamilySymbol;
                if (null != tn)
                {
                    if(tn.Category.Id.IntegerValue == (int) BuiltInCategory.OST_RebarTags)
                    m_data.Tags.Add(tn);
                }
            }

            ElementId tag_default = doc.GetDefaultFamilyTypeId(new ElementId((int)BuiltInCategory.OST_RebarTags));
            m_data.Tag_default = m_data.Tags.FindIndex(x => x.Id.IntegerValue == tag_default.IntegerValue);

        }
            /// <summary>
            /// Получить данные по стилям линий и шрифтам
            /// </summary> 
            void GetLineStyles()
        {
            
            Transaction t = new Transaction(doc,"Get data from project");
            t.Start();
            Line line = Line.CreateBound(new XYZ(0,0,0), new XYZ(1, 0, 0));


            Autodesk.Revit.DB.Plane geometryPlane = Autodesk.Revit.DB.Plane.CreateByOriginAndBasis(new XYZ(0, 0, 0), new XYZ(1, 0, 0), new XYZ(0, 1, 0));
            SketchPlane skplane = SketchPlane.Create(doc, geometryPlane);
            ModelCurve mcurve = doc.Create.NewModelCurve(line, skplane);
            String line_style = mcurve.LineStyle.Name;
            doc.Regenerate();             
            ICollection<ElementId> lsArr = mcurve.GetLineStyleIds();
            t.RollBack();

            int i = 0;
            
            foreach (ElementId eid in lsArr)
            {

                GraphicsStyle cat = doc.GetElement(eid) as GraphicsStyle;              
                if (cat.GraphicsStyleCategory.Id.IntegerValue == (int)BuiltInCategory.OST_CurvesMediumLines)
                    m_data.Line_types_default = i;
                m_data.Line_types.Add(cat);
                i++;
            }

            ElementId font_default=  doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            FilteredElementIterator List_fonts = collector2.WherePasses(new ElementClassFilter(typeof(TextNoteType))).GetElementIterator();    // .OfType<Element>().ToList();
            List_fonts.Reset();
            while (List_fonts.MoveNext())
            {
                //добавим тип линии к списку 
                TextNoteType tn = List_fonts.Current as TextNoteType;
                if (null != tn)
                {
                    m_data.Fonts.Add(tn);
                }
            }

            m_data.Font_default = m_data.Fonts.FindIndex(x => x.Id.IntegerValue == font_default.IntegerValue);

            return;
        }

    }
        

    public class dataManager
    {         
        public int Tag_default = 0;
        public int Line_types_default = 0;
        public int Line_types_test = 0;
        public int Font_default = 0;
        public List<GraphicsStyle> Line_types = new List<GraphicsStyle>();
        public List<TextNoteType> Fonts = new List<TextNoteType>();
        public List<FamilySymbol> Tags = new List<FamilySymbol>();
        /// <summary>
        /// Направление армирования для площади 
        /// </summary>
        public AreaDirect areaDirect = AreaDirect.Main;
        /// <summary>
        /// Слой армирования для площади 
        /// </summary>
        public AreaLayer areaLayer = AreaLayer.Up;
        /// <summary>
        /// Режим использования существующих марок 
        /// </summary>
        public bool UsePrefix=true;
        /// <summary>
        /// Режим генерации эскизов: все, отдельно, по направлению
        /// </summary>
        public Mode SketchMode = Mode.MainSegment;
        /// <summary>
        /// Направление расстановки эскизов 
        /// </summary>
        public SketchDirect Direct = SketchDirect.Down;
        /// <summary>
        /// Режим выравнивания эскизов 
        /// </summary>
        public bool Aligment = true;
        /// <summary>
        /// Признак удаления избыточных эскизов 
        /// </summary>
        public bool Clear = true;
        /// <summary>
        /// Масштаб отображения эскизов 
        /// </summary>
        public double Scale=1.0;
        /// <summary>
        /// Признак добавления общей длины стержня 
        /// </summary>
        public bool IsTotalLength = true;
    }
    
}
