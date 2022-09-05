using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace SketchFull
{
    /// <summary>
    /// Построение чертежа 
    /// </summary>
    public class BuildSketchRebar
    {
        #region Параметры
        /// <summary>
        /// Базовый вид
        /// </summary>
        public Autodesk.Revit.DB.View active_view = null;
        /// <summary>
        /// Находится ли стержень в плоскости чертежа
        /// </summary>
        public bool IsRebarOnViewPlane
        {
            get
            {
                XYZ view_normal = active_view.ViewDirection;

                if (rarc != null)
                {
                    if (rarc.Type == RebarShapeDefinitionByArcType.Spiral)
                    {
                        if (SketchTools.CompareXYZ(ZRebarNormal, view_normal) || SketchTools.CompareXYZ(ZRebarNormal.Negate(), view_normal)) return false;
                        else return true;
                    }
                }

                return (SketchTools.CompareXYZ(ZRebarNormal, view_normal) || SketchTools.CompareXYZ(ZRebarNormal.Negate(), view_normal));
            }
        }
        /// <summary>
        ///// Начало координат группы
        ///// </summary>
        //public XYZ initial_group;
        ///// <summary>
        ///// Статус создания стержня
        ///// </summary>
        //public bool status = true;
        ///// <summary>
        ///// Текущая транзакция
        ///// </summary>
        //public Transaction transaction;
        ///// <summary>
        ///// Текущий шаблон проекта
        ///// </summary>
        //public Template template;
        ///// <summary>
        ///// Чертеж
        ///// </summary>
        //public Graphics graphic;
        /// <summary>
        /// Элемент арматурного стержня
        /// </summary>
        public Element rebar;
        /// <summary>
        /// Элемент арматурного стержня
        /// </summary>
        public Rebar rebarOne
        {
            get { return rebar as Rebar; }
        }
        /// <summary>
        /// Элемент арматурного стержня
        /// </summary>
        public RebarInSystem rebarIn
        {
            get { return rebar as RebarInSystem; }
        }
        //public GeometryElement geometryElement
        //{
        //    get
        //    {
        //        if (rebarOne != null)
        //        {
        //            return rebarOne.get_Geometry(new Options());
                     
        //        }
        //        if (rebarIn != null)
        //        {
        //            return rebarOne.get_Geometry(new Options());
        //        }
        //        return null;
        //    }
        //}
        ///// <summary>
        ///// Размер рисунка по оси Х
        ///// </summary>
        //public int sizeX = 1000;              // по умолчанию
        ///// <summary>
        ///// Размер рисунка по оси Y
        ///// </summary>
        //public int sizeY = 300;               // по умолчанию
        ///// <summary>
        ///// Размер шрифта
        ///// </summary>
        //public float move = 90;             // по умолчанию
        ///// <summary>
        ///// Размер канвы
        ///// </summary>
        //public float canva = 63;            // по умолчанию
        ///// <summary>
        ///// Коэффициент перевода единиц
        ///// </summary>
        //const float unit = (float)0.00328;
        #endregion Параметры

        #region Инициализация массивов

        // public StreamWriter writer;

        ///// <summary>
        ///// Список текстовых надписей
        ///// </summary>
        //public List<TextNote> Notes = new List<TextNote>();
        /// <summary>
        /// Список элементов для включения в группу чертежа
        /// </summary>
        public ICollection<ElementId> Eids = new List<ElementId>();
        /// <summary>
        /// Список кривых на чертеже
        /// </summary>
        List<DetailCurve> detailCurves = new List<DetailCurve>();    
        /// <summary>
        /// Список элементов для поворота
        /// </summary>
        ICollection<ElementId> elements_rotate = new List<ElementId>();
        ///// <summary>
        ///// Список элементов для поворота
        ///// </summary>
        //ICollection<ElementId> elements_rotate2 = new List<ElementId>();
        ///// <summary>
        ///// Список элементов для поворота
        ///// </summary>
        //// ICollection<ElementId> elements_rotate3 = new List<ElementId>();
        ///// <summary>
        ///// Список текстовых надписей для поворота
        ///// </summary>
        //ICollection<ElementId> text_rotate = new List<ElementId>();
        /// <summary>
        /// Список элементов для включения в группу чертежа (только линии)
        /// </summary>
        public ICollection<ElementId> Eids_lines = new List<ElementId>();
        ///// <summary>
        ///// Видимость длины крюков
        ///// </summary>
        //public bool hooks_length = true;
        ///// <summary>
        ///// Видимость радиусов загиба
        ///// </summary>
        //public bool bending = false;
        ///// <summary>
        ///// Коэффициенты для крюков
        ///// </summary>
        //public double coef_hook = 1;
        ///// <summary>
        ///// Коэффициенты для сегментов стержня
        ///// </summary>
        //public double[] coef = { 1, 1, 1, 1, 1, 1, 1 };
        /// <summary>
        /// Параметры для крюков
        /// </summary>
        public List<TextOnRebar> hooks = new List<TextOnRebar>();
        /// <summary>
        /// Линии сегментов стержня возле крюков
        /// </summary>
        Plane3D work_plane_drawing = null;
        /// <summary>
        /// Линии сегментов стержня возле крюков
        /// </summary>
        public List<Curve> cross_lines_hooks = new List<Curve>();
        ///// <summary>
        ///// Линии чертежа (только прямые)
        ///// </summary>
        //public List<Line2D> line2D_L = new List<Line2D>();                                       // список плоских линий для чертежа (только прямые)
        ///// <summary>
        ///// Линии чертежа
        ///// </summary>
        //public List<Line2D> line2D = new List<Line2D>();                                       // список плоских линий для чертежа
        ///// <summary>
        ///// Линии арматуры
        ///// </summary>
        //public List<PointF> pointDF = new List<PointF>();
        /// <summary>
        /// Список параметров для прямых сегментов
        /// </summary> 
        public List<TextOnRebar> lg = new List<TextOnRebar>();
        /// <summary>
        /// Линия ограничивающая эскиз
        /// </summary> 
        public Line line_up;
        /// <summary>
        /// Линия ограничивающая эскиз
        /// </summary> 
        public Line line_down;
        /// <summary>
        /// Линия ограничивающая эскиз
        /// </summary> 
        public Line line_left;
        /// <summary>
        /// Линия ограничивающая эскиз
        /// </summary> 
        public Line line_right;
        ///// <summary>
        ///// Текстовые надписи (радиусы)
        ///// </summary>
        //public List<TextOnArc> lg_arc_sorted = new List<TextOnArc>();
        ///// <summary>
        ///// Линии чертежа
        ///// </summary>
        //public List<TextOnArc> lg_arc = new List<TextOnArc>();
        ///// <summary>
        ///// Надписи над отрезками
        ///// </summary>
        //public List<TextOnRebar> Llg = new List<TextOnRebar>();
        ///// <summary>
        ///// Вариант вычерчивания: 0 - ортогонально виду или 1
        ///// </summary>
        //int variant_drawing = 0;
        ///// <summary>
        ///// Базовый сегмент стержня
        ///// </summary>
        //int base_segment = -1;
        /// <summary>
        /// Cписок сегментов для стержня проекта
        /// </summary>
        IList<Curve> ilc = new List<Curve>();
        /// <summary>
        /// Cписок прямых сегментов для стержня проекта
        /// </summary>
        IList<Curve> ilc_lines = new List<Curve>();
        /// <summary>
        /// Cписок сегментов для стержня проекта в системе координат текущего вида
        /// </summary>
        IList<Curve> ilc_transform = new List<Curve>();
        /// <summary>
        /// Cписок сегментов для стержня проекта в системе координат текущего вида - без изменений связанных с поворотом
        /// </summary>
        IList<Curve> ilc_transform_initial = new List<Curve>();
        ///// <summary>
        ///// Cписок сегментов для стержня проекта в системе координат временного вида
        ///// </summary>
        //IList<Curve> ilc_transform_temp = new List<Curve>();
        ///// <summary>
        ///// Временный вид для построения чертежа
        ///// </summary>
        //ViewSection vs = null;
        /// <summary>
        /// Признак спирального эскиза
        /// </summary>
        bool IsHermiteSpline = false;


        #endregion Инициализация массивов

        ///// <summary>
        ///// Файл рисунка
        ///// </summary>
        //public Bitmap flag;
        //{
        //    get
        //    {
        //        return new Bitmap(sizeX,sizeY);
        //    }           
        //}
        /// <summary>
        /// Признак правильного создания стержня
        /// </summary>
        public bool IsRebarCorrect=false;
        /// <summary>
        /// Направление основного сегмента в модели
        /// </summary>
        public XYZ DirMainSegment = null;
        /// <summary>
        /// Индекс основного сегмента в модели
        /// </summary>
        public int IndexMainSegment = -1;
        ///// <summary>
        ///// Направление основного сегмента в группе
        ///// </summary>
        //public XYZ DirMainSegmentGroup = null;
        ///// <summary>
        ///// Элемент основного сегмента в группе
        ///// </summary>
        //public DetailCurve MainDetailCurve = null;

        /// <summary>
        /// Линия основного сегмента в группе
        /// </summary>
        public Line MainLine = null;

        /// <summary>
        /// Линия основного сегмента в группе в масштабе
        /// </summary>
        public Line MainLineScale = null;


        ///// <summary>
        ///// Направление основного сегмента в группе
        ///// </summary>
        //public XYZ DirMainDetailCurve
        //{
        //    get
        //    {
        //        if(MainDetailCurve!=null)
        //        {
        //            Curve main_on_view = MainDetailCurve.GeometryCurve;
        //            return (main_on_view.GetEndPoint(1) - main_on_view.GetEndPoint(0)).Normalize();  // направление основного сегмента на чертеже
        //        }
        //        return null;
        //    }
        //}
        /// <summary>
        /// Проекция середины основного сегмента на текущем виде
        /// </summary>
        public XYZ ProjectMainSegmentOnView = null;

        /// <summary>
        /// Направление оси Z - перпендикулярно плоскости стержня
        /// </summary>
        XYZ ZRebarNormal
        {
            get
            {
                if (rebarOne != null)
                {
                    //if (rarc != null)
                    //{
                    //    if (rarc.Type == RebarShapeDefinitionByArcType.Spiral)
                    //    {
                    //        return (ilc[0].GetEndPoint(1) - ilc[0].GetEndPoint(0)).Normalize();                            
                    //    }
                    //}
                    try
                    {
                        return rebarOne.GetShapeDrivenAccessor().Normal;
                        // return SketchTools.GetNormalVector(ilc);
                    }
                    catch
                    {
                        if (rebarOne.IsRebarFreeForm())
                        {                            
                                return SketchTools.GetNormalVector(ilc).Normalize();                             
                        }
                            else  return XYZ.Zero;                        
                    }
                }
                if (rebarIn != null)
                {

                    return rebarIn.Normal;
                }
                return XYZ.Zero;
            }
        }
        /// <summary>
        /// Направление оси Z - перпендикулярно плоскости стержня
        /// </summary>
        Vector4 zAxis  
        {
            get
            {                
                 
                if (rebarOne != null)
                {

                    return new Vector4(rebarOne.GetShapeDrivenAccessor().Normal);
                }
                if (rebarIn != null)
                {

                    return new Vector4(rebarIn.Normal);
                }
                return new Vector4(XYZ.Zero);
            }
        }
        /// <summary>
        /// Диаметр стержня - текстовое значение
        /// </summary>
        string Diametr_str
        {
            get
            {
                return SketchTools.GetRoundDiametrRebar(rebar);
            }

        }
        /// <summary>
        /// Полная длина стержня - текстовое значение
        /// </summary>
        string Length_str
        {
            get
            {
                return SketchTools.GetRoundFullLengthRebar(rebar);
            }

        }
        /// <summary>
        /// Диаметр стержня
        /// </summary>
        double Diametr
        {
            get
            {

                if (rebarOne != null)
                {
                    
                    return Math.Round(rebarOne.GetBendData().BarModelDiameter,3);
                }
                else
                {

                    return Math.Round(rebarIn.GetBendData().BarModelDiameter,3);
                }                 
            }
        }
        ///// <summary>
        ///// Максимум модели по оси Х
        ///// </summary>
        //public float maxX = 1;
        ///// <summary>
        ///// Максимум модели по оси Y
        ///// </summary>
        //public float maxY = 1;
        ///// <summary>
        ///// Минимум модели по оси Х
        ///// </summary>
        //public float minX = 1;
        ///// <summary>
        ///// Минимум модели по оси Y
        ///// </summary>
        //public float minY = 1;
        ///// <summary>
        ///// Коэффициент масштаба
        ///// </summary>
        //float scale
        //{
        //    get
        //    {
        //        float scaleX = (float)((sizeX - 2 * canva) / maxX);
        //        float scaleY = (float)(sizeY - 2 * canva) / maxY;
        //        return Math.Min(scaleX, scaleY);
        //    }
        //}
        ///// <summary>
        ///// Сдвиг по оси Х
        ///// </summary>
        //public float moveX;
        ///// <summary>
        ///// Сдвиг по оси Y
        ///// </summary>
        //public float moveY;
        /// <summary>
        /// Текущий документ
        /// </summary>
        Document doc;
        /// <summary>
        /// Форма стержня
        /// </summary>
        RebarShape rs = null;
        RebarShapeDefinition rsd = null;
        RebarShapeDefinitionByArc rarc = null;
        RebarShapeDefinitionBySegments rsds = null;
        /// <summary>
        /// Крюк в начале стержня
        /// </summary>
        int hook_start
        {
            get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId().IntegerValue; }
        }
        /// <summary>
        /// Крюк в начале стержня
        /// </summary>
        int hook_end
        {
            get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId().IntegerValue; }
        }
                 
        /// <summary>
        /// Зона крюка в начале стержня
        /// </summary>
        Outline hook_start_outline;
        /// <summary>
        /// Зона крюка в конце стержня
        /// </summary>
        Outline hook_end_outline;

        /// <summary>
        /// Номер начального кривой (без крюка)
        /// </summary>
        int curve_start
        {
            get
            {
                if (hook_start > 0) return 2;
                else return 0;
            }
        }

        /// <summary>
        /// Номер конечной кривой (без крюка)
        /// </summary>
        int curve_end
        {
            get
            {
                if (hook_end > 0) return ilc.Count - 2;
                else return ilc.Count;
            }
        }

        ///// <summary>
        ///// Коэффициент минимальной длины по крюку
        ///// </summary>
        //int min = 5;
        ///// <summary>
        ///// Коэффициент максимальной длины по крюку
        ///// </summary>
        //int max = 15;
        ///// <summary>
        ///// Начальная точка
        ///// </summary>
        //XYZ p_initial = null;
        ///// <summary>
        ///// Основное направление - по оси Х
        ///// </summary>
        //XYZ dir_major = null;
        ///// <summary>
        ///// Коэффициент шрифта по высоте
        ///// </summary>
        //public double coeff_font=0.25;
        // GraphicsStyle gs;
        GraphicsStyle gs_test;
        TextNoteType tnt;
        FamilySymbol rebar_tag;
        /// <summary>
        /// Рабочая плоскость
        /// </summary>
        Plane3D plane3D = null;
        ///// <summary>
        ///// Точка на плоскости вида
        ///// </summary>
        //XYZ p1View = null;
        ///// <summary>
        ///// Точка на плоскости вида
        ///// </summary>
        //XYZ p2View = null;
        ///// <summary>
        ///// Точка на плоскости вида
        ///// </summary>
        //XYZ p3View = null;
        /// <summary>
        /// Внешний контур эскиза
        /// </summary>
        Outline outline =null;
        /// <summary>
        /// Точки в системе координат временного вида
        /// </summary>
        Transform temp_view = Transform.Identity;
        /// <summary>
        /// Точки в системе координат текущего вида
        /// </summary>
        public Transform to_current_view = Transform.Identity;
        /// <summary>
        /// Масштаб эскиза
        /// </summary>
        public double scale=1.0;
        /// <summary>
        /// Признак добавления диаметра и длині
        /// </summary>
        public bool IsTotalLength = true;


        public BuildSketchRebar(Element rebar,Plane3D plane3D, GraphicsStyle gs_test, TextNoteType tnt, FamilySymbol rebar_tag)
        {
            this.rebar = rebar;
            this.doc = rebar.Document;
            // this.gs = gs;
            this.gs_test = gs_test;
            this.tnt = tnt;
            this.rebar_tag = rebar_tag;           
            this.plane3D = plane3D;
            //// получить три точки на плоскости вида
            //p1View = active_view.Origin;
            //p2View = active_view.RightDirection + p1View;
            //p3View = active_view.UpDirection + p1View;            
        }
        //public BuildSketchRebar(Element rebar)
        //{
        //    this.rebar = rebar;
        //    // получить три точки на плоскости вида
        //    //p1View = active_view.Origin;
        //    //p2View = active_view.RightDirection + p1View;
        //    //p3View = active_view.UpDirection + p1View;
        //}
        //public BuildSketchRebar(Element rebar, Template template, GraphicsStyle gs, TextNoteType tnt, FamilySymbol rebar_tag)
        //{
             
        //    this.rebar = rebar;
        //    this.template = template;
        //    this.doc = rebar.Document;
        //    XYZ move = XYZ.Zero;

        //    // разделить по типам стержней, получить кривые контура стержня и вектор Z
        //    DivideByTypeRebar();
        //    TransformToNewCoordinateSystem();
        //    if (ilc_transform.Count > 0)
        //    {
        //        // текущий чертеж
        //        Outline outline = null;

        //        if (variant_drawing == 1)  // для неортогонального положения - смещаем и поворачиваем чертеж
        //        {
        //            XYZ initial_old = ilc[base_segment].GetEndPoint(0);                                              // начало системы координат для перемещения
        //            XYZ initial_vector_old = (ilc[base_segment].GetEndPoint(1) - ilc[base_segment].GetEndPoint(0));  // направление базового участка
        //            XYZ initial_new = ilc_transform[base_segment].GetEndPoint(0);
        //            move = initial_old - initial_new;                        // вектор перемещения

        //            Line axis = Line.CreateUnbound(initial_old, active_view.ViewDirection);   // ось вращения

        //            DetailCurve newcurve0 = doc.Create.NewDetailCurve(active_view, ilc_transform[base_segment]);
        //            newcurve0.Location.Move(move);
        //            Curve curve = newcurve0.GeometryCurve;
        //            XYZ initial_vector_new = (curve.GetEndPoint(1) - curve.GetEndPoint(0));
        //            double angle = -initial_vector_new.AngleTo(initial_vector_old);
        //            newcurve0.Location.Rotate(axis, angle);
        //            doc.Delete(newcurve0.Id);

        //            for (int i = 0; i < ilc_transform.Count; i++)
        //            {
        //                DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, ilc_transform[i]);
        //                newcurve.Location.Move(move);
        //                newcurve.Location.Rotate(axis, angle);

        //                Curve c = newcurve.GeometryCurve;
        //                ilc_transform[i] = c;                    // теперь кривая такая и здесь

        //                newcurve.LineStyle = gs;
        //                Eids_lines.Add(newcurve.Id);
        //                Eids.Add(newcurve.Id);                     // все кривые показываем в группе чертежа
        //                if (outline == null)
        //                {
        //                    outline = new Outline(ilc_transform[0].GetEndPoint(0), ilc_transform[0].GetEndPoint(1));
        //                }
        //                else
        //                {
        //                    outline.AddPoint(ilc_transform[i].GetEndPoint(0));
        //                    outline.AddPoint(ilc_transform[i].GetEndPoint(1));
        //                }
        //            }

        //        }

        //        else
        //        {
        //            // выполняем вычерчивание основных линий арматурного стержня
        //            foreach (Curve curve in ilc_transform)
        //            {
        //                DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, curve);
        //                newcurve.LineStyle = gs;
        //                Eids_lines.Add(newcurve.Id);
        //                Eids.Add(newcurve.Id);                     // все кривые показываем в группе чертежа
        //                if (outline == null)
        //                {
        //                    outline = new Outline(curve.GetEndPoint(0), curve.GetEndPoint(1));
        //                }
        //                else
        //                {
        //                    outline.AddPoint(curve.GetEndPoint(0));
        //                    outline.AddPoint(curve.GetEndPoint(1));
        //                }
        //            }
        //        }

        //        GetInfoAboutHooks();                           // получить данные по крюкам
        //                                                       // запишем данные по гнутым участкам
        //        lg_arc.Clear();
        //        for (int i = curve_start; i < curve_end; i++)
        //        {
        //            Curve c = ilc[i];
        //            // гнутые участки записываем для указания радиуса загиба
        //            if (c.GetType().Name == "Arc") lg_arc.Add(GetArcSegment(ilc, rebar, i));        // для участка типа дуга
        //        }
        //        DataBySegments();                                // Формирование данных по участкам армирования
        //        IsRebarCorrect = InitialDataForSegments();       // инициализация данных для сегментов

        //        //if (move != XYZ.Zero)
        //        //{
        //        //    for (int i = 0; i < lg.Count; i++)
        //        //    {
        //        //        lg[i].position = lg[i].position + move;
        //        //    }
        //        //}

        //        if (IsRebarCorrect)
        //        {

        //            Line line_up = Line.CreateUnbound(outline.MaximumPoint, active_view.RightDirection);
        //            Line line_down = Line.CreateUnbound(outline.MinimumPoint, active_view.RightDirection);

        //            Line line_left = Line.CreateUnbound(outline.MaximumPoint, active_view.UpDirection);
        //            Line line_rigth = Line.CreateUnbound(outline.MinimumPoint, active_view.UpDirection);

        //            XYZ vec = (outline.MinimumPoint - outline.MaximumPoint).Normalize();

        //            if (active_view.RightDirection.AngleTo(vec) > Math.PI / 2)
        //            {
        //                line_rigth = Line.CreateUnbound(outline.MaximumPoint, active_view.UpDirection);
        //                line_left = Line.CreateUnbound(outline.MinimumPoint, active_view.UpDirection);

        //            }

        //            foreach (TextOnRebar label in lg)
        //            {
        //                label.angle = active_view.RightDirection.AngleTo(label.dir_segment);
        //                TextNote tn = TextNote.Create(doc, active_view.Id, label.position, label.value_str, tnt.Id);
        //                doc.Regenerate();
        //                if (label.angleI != 0)
        //                {
        //                    Line axes = Line.CreateUnbound(label.position, active_view.ViewDirection);
        //                    tn.Location.Rotate(axes, label.angleI);
        //                    doc.Regenerate();
        //                    // точка вставки текста  
        //                    if (line_left.Distance(label.position) < line_rigth.Distance(label.position))
        //                    {
        //                        // левая метка                                
        //                        tn.Location.Move(tn.UpDirection * active_view.Scale * tn.Height * 1.05);
        //                    }
        //                    else
        //                    {
        //                        // правая метка
        //                        tn.Location.Move(tn.UpDirection.Negate() * active_view.Scale * tn.Height * 0.05);
        //                    }




        //                    // точка вставки текста  
        //                    if (label.dir_segment.AngleTo(active_view.UpDirection) <= Math.PI / 2)
        //                    {
        //                        // вверх                                
        //                        tn.Location.Move(label.dir_segment.Negate() * active_view.Scale * tn.Width * 0.5);
        //                    }
        //                    else
        //                    {
        //                        // вниз
        //                        tn.Location.Move(label.dir_segment * active_view.Scale * tn.Width * 0.5);
        //                    }
        //                }
        //                else
        //                {
        //                    if (line_up.Distance(label.position) < line_down.Distance(label.position))
        //                    {
        //                        // верхняя метка                                
        //                        tn.Location.Move(active_view.UpDirection * active_view.Scale * tn.Height * 1.05);
        //                    }
        //                    else
        //                    {
        //                        // нижняя метка
        //                        tn.Location.Move(active_view.UpDirection.Negate() * active_view.Scale * tn.Height * 0.05);
        //                    }
        //                    tn.Location.Move(tn.BaseDirection.Negate() * active_view.Scale * tn.Width / 2);
        //                }
        //                Notes.Add(tn);
        //                Eids.Add(tn.Id);
        //            }

        //            //добавить метку
        //            if (rebar_tag != null)
        //            {
        //                IndependentTag it = IndependentTag.Create(doc, active_view.Id, new Reference(rebar), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, outline.MaximumPoint);
        //                it.ChangeTypeId(rebar_tag.Id);
        //                string text = it.TagText;
        //                doc.Regenerate();
        //                doc.Delete(it.Id);
        //                TextNote tn = TextNote.Create(doc, active_view.Id, outline.MaximumPoint, text, tnt.Id);
        //                doc.Regenerate();
        //                tn.Location.Move(active_view.UpDirection * active_view.Scale * tn.Height * 1.05);
        //                Notes.Add(tn);
        //                Eids.Add(tn.Id);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Подготовить сегменты стержня для построения чертежа
        /// </summary>
        public bool PreparedDataSegements()
        {
            
            GetRebarsCurves();   // получим кривые стержня
           
            if (!TransformToNewCoordinateSystem()) return false; // получим кривые в новой системе координат
            
            PreparedCurves();    // подготовим кривые для вычерчивания
           
            return true;
        }

        void PreparedCurves()
        {
            //GraphicsStyle graphicsStyle = GetStyleLine();   // получить тип линии
            //if (graphicsStyle != null) gs_test = graphicsStyle;
            
            XYZ move = XYZ.Zero;
            if (ilc_transform.Count > 0)
            {
                // вычертим замыкающие линии
                if (!IsHermiteSpline)
                {
                    CreateOffSetCurveOnEnds(ilc_transform[0], 0);
                    CreateOffSetCurveOnEnds(ilc_transform[ilc_transform.Count - 1], 1);
                }
              
                // выполняем вычерчивание основных линий арматурного стержня
                for (int i=0; i<ilc_transform.Count;i++)
                {
                    Curve curve = ilc_transform[i];

                    if (IsHermiteSpline)
                    {                        
                        CreateNewCurvesOnView(curve, IndexMainSegment == i,false);                                                         
                    }
                    else
                    {                        
                        CreateNewCurvesOnView(curve,IndexMainSegment==i);
                    }

                }
                
                GetInfoAboutHooks();                                // получить данные по крюкам 

                if (rsds == null && rarc == null) return;   // формы не определяются

                if(rsds!=null)             // Формирование данных по участкам армирования
                {
                    DataBySegments();
                    IsRebarCorrect = InitialDataForSegments();       // инициализация данных для сегментов
                }
                if (rarc != null)          // Формирование данных по участкам армирования
                {
                    DataByArcs();
                    if (lg.Count > 0) IsRebarCorrect = true;
                }

                if (IsRebarCorrect)
                {
                    // лучи ограничивающие контур эскиза
                    line_up = Line.CreateUnbound(outline.MaximumPoint, active_view.RightDirection);
                    line_down = Line.CreateUnbound(outline.MinimumPoint, active_view.RightDirection);

                    line_left = Line.CreateUnbound(outline.MaximumPoint, active_view.UpDirection);
                    line_right = Line.CreateUnbound(outline.MinimumPoint, active_view.UpDirection);

                    XYZ vec = (outline.MinimumPoint - outline.MaximumPoint).Normalize();

                    if (active_view.RightDirection.AngleTo(vec) > Math.PI / 2)
                    {
                        line_right = Line.CreateUnbound(outline.MaximumPoint, active_view.UpDirection);
                        line_left = Line.CreateUnbound(outline.MinimumPoint, active_view.UpDirection);

                    }

                    List<TextOnRebar> createdText = new List<TextOnRebar>();

                    // текст на прямых участках
                    for (int i = 0; i < lg.Count; i++)
                    {

                        TextOnRebar label = lg[i];

                        bool find_text = false;
                        // тексты сходного направления и значение - пропускаем
                        foreach (TextOnRebar tor in createdText)
                        {
                            if (tor.value_str == label.value_str)
                            {
                                if (Math.Round(tor.dir_segment_initial.AngleTo(label.dir_segment_initial), 3) == 0.00 ||
                                    Math.Round(tor.dir_segment_initial.AngleTo(label.dir_segment_initial), 3) == 3.142)
                                {
                                    find_text = true; break;
                                }
                            }
                        }
                        if (find_text) continue;

                        label.angle = label.GetAngleForTextNote(label.dir_segment, active_view.RightDirection, active_view.UpDirection);

                        TextNote tn = TextNote.Create(doc, active_view.Id, label.position, label.value_str, tnt.Id);
                        doc.Regenerate();

                        CreateTextNote(label, tn);
                        createdText.Add(label);
                        Eids.Add(tn.Id);

                    }
                    // текст на крюках
                    foreach (TextOnRebar label in hooks)
                    {
                        if (hooks.Count() == 2)
                        {
                            // при одинаковом значении длины крюков - последний не показываем
                            if (label.isHookEnd && hooks[0].value == hooks[1].value) continue;
                        }

                        label.angle = label.GetAngleForTextNote(label.dir_segment, active_view.RightDirection, active_view.UpDirection);

                        TextNote tn = TextNote.Create(doc, active_view.Id, label.position, label.value_str, tnt.Id);
                        doc.Regenerate();
                        CreateTextNoteOnHook(label, tn);
                        Eids.Add(tn.Id);
                    }

                    if (detailCurves.Count > 0)
                    {
                        doc.Delete(Eids_lines);
                    }

                    int count_rebars = GetCountRebars();

                    string text = "";
                    
                    // текст по умолчанию - диаметр + полная длина стержня
                    if (IsTotalLength)
                    {
                        if (count_rebars > 1) text = " (" + count_rebars.ToString() + ") ";
                        text = text + "Ø" + Diametr_str + " L=" + Length_str;
                    }

                    if (rebar_tag != null)
                    {
                        IList<Subelement> subelements = rebar.GetSubelements();
                        IndependentTag it = IndependentTag.Create(doc, active_view.Id, subelements[0].GetReference(), true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, outline.MinimumPoint);
                        // IndependentTag it = IndependentTag.Create(doc, rebar_tag.Id, active_view.Id, new Reference(rebar), false, TagOrientation.Horizontal, outline.MinimumPoint);                        
                        it.ChangeTypeId(rebar_tag.Id);
                        doc.Regenerate();
                        text = it.TagText + " " + text;
                        doc.Delete(it.Id);
                    }
                    if (text.Length > 0)
                    { 
                        // точка вставки: вверху, в середине контура
                        outline.MaximumPoint = SketchTools.ProjectPointOnWorkPlane(plane3D, outline.MaximumPoint);
                        outline.MinimumPoint = SketchTools.ProjectPointOnWorkPlane(plane3D, outline.MinimumPoint);
                        XYZ cross = outline.MaximumPoint;
                        XYZ middle = (outline.MaximumPoint + outline.MinimumPoint) / 2;                        
                        Line rawY = Line.CreateUnbound(middle, active_view.UpDirection);
                        Line rawX = Line.CreateUnbound(outline.MinimumPoint, active_view.RightDirection);

                        IntersectionResultArray ira = null;
                        rawY.Intersect(rawX, out ira);
                        if (ira != null)
                        {
                            cross = ira.get_Item(0).XYZPoint;
                            if (cross == null) cross = outline.MinimumPoint;
                        }

                        // текстовая надпись - строго горизонтальная
                        TextNote tn = TextNote.Create(doc, active_view.Id, cross, text, tnt.Id);                         
                        doc.Regenerate();
                       
                        tn.Location.Move(active_view.UpDirection.Negate() * active_view.Scale * tn.Height * 0.2);
                        tn.Location.Move(active_view.RightDirection.Negate() * active_view.Scale * tn.Width / 2);
                        
                        Eids.Add(tn.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Получить число стержней в жб элемента
        /// </summary>
        int GetCountRebars()
        {
            // выбрать все подобные стержни
            FilteredElementCollector collectorS = new FilteredElementCollector(doc); // .OfClass(typeof(Rebar));
            // критерии выбора:
            // принадлежность к жб элементу
            // диаметр и класс
            // форма стержня
            // длина стержня

            if (rebarOne != null)
            {
                List<Rebar> all_rebars = new List<Rebar>();
                all_rebars = collectorS.WherePasses(new ElementClassFilter(typeof(Rebar))).OfType<Element>().Where(
                        x => (x as Rebar).GetHostId().IntegerValue== rebarOne.GetHostId().IntegerValue &&
                        doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == doc.GetElement(rebarOne.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() &&
                        doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name == doc.GetElement(rebarOne.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name &&
                        Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0) == Math.Round(rebarOne.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0)).ToList().ConvertAll<Rebar>(x=> x as Rebar);
                if (all_rebars.Count == 0) return 0;

                int count_rebarOne = 0;
                foreach (Rebar bar in all_rebars)
                {
                    count_rebarOne = count_rebarOne + bar.get_Parameter(BuiltInParameter.REBAR_QUANITY_BY_DISTRIB).AsInteger();
                }
                return count_rebarOne;
            }
            if(rebarIn !=null)
            {
                collectorS = new FilteredElementCollector(doc);
                List<RebarInSystem> all_rebars_InSystem = new List<RebarInSystem>();
                        all_rebars_InSystem = collectorS.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).OfType<Element>().Where(
                        x => (x as RebarInSystem).GetHostId().IntegerValue == rebarIn.GetHostId().IntegerValue  &&
                        doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == doc.GetElement(rebarIn.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() &&
                        doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name == doc.GetElement(rebarIn.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name &&
                        Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0) == Math.Round(rebarIn.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0)).ToList().ConvertAll<RebarInSystem>(x => x as RebarInSystem);
                if (all_rebars_InSystem.Count == 0) return 0;

                int count_rebarIn = 0;
                foreach (RebarInSystem barIn in all_rebars_InSystem)
                {
                    count_rebarIn = count_rebarIn + barIn.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS).AsInteger();
                }
                return count_rebarIn;
            }
            return 0;
        }       

        /// <summary>
        /// Создаем новую кривую на чертеже
        /// </summary>
        void CreateNewCurvesOnView(Curve curve, bool IsMainIndex, bool show_add_line=true)
        {

            try
            {
                DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, curve);
                newcurve.LineStyle = gs_test;
                Curve c = newcurve.GeometryCurve;
                Eids_lines.Add(newcurve.Id);
                Eids.Add(newcurve.Id);                     // все кривые показываем в группе чертежа

                if (outline == null)
                {
                    outline = new Outline(c.GetEndPoint(0), c.GetEndPoint(1));
                }
                else
                {
                    outline.AddPoint(c.GetEndPoint(0));
                    outline.AddPoint(c.GetEndPoint(1));
                }

                if (show_add_line) CreateOffSetCurve(curve, IsMainIndex);                  // создаем линии чертежа по внешнему контуру
            }
            catch { }

        }

        /// <summary>
        /// Найти тип стиля линии для отображения стержня
        /// </summary>
        GraphicsStyle GetStyleLine()
        {
            GraphicsStyle gs_new = null;
            ElementId eid_type = rebar.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId();
            RebarBarType rbt = doc.GetElement(eid_type) as RebarBarType;
            string name_drawing_style = rbt.get_Parameter(BuiltInParameter.REBAR_BAR_STYLE).AsValueString();

            IList<ElementFilter> filterList_element = new List<ElementFilter>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector = new FilteredElementCollector(doc);

            filterList_element.Clear();
            // filterList_element.Add(new ElementCategoryFilter(BuiltInCategory.OST_Rebar));
            filterList_element.Add(new ElementClassFilter(typeof(GraphicsStyle)));
            LogicalAndFilter filter_element = new LogicalAndFilter(filterList_element);

            IList<Element> elements = collector.WherePasses(filter_element).ToElements();

            foreach (GraphicsStyle el in elements)
            {
                if (el.GraphicsStyleCategory.Parent == null) continue;
                if ( el.GraphicsStyleCategory.Parent.Id.IntegerValue != (int) BuiltInCategory.OST_Rebar ) continue;
                if (el.Name == name_drawing_style)
                {
                    gs_new = el as GraphicsStyle;
                    break;
                }

            }

            return gs_new;

            // Options options = new Options();
            //options.IncludeNonVisibleObjects = true;
            //options.ComputeReferences = true;
            //// GeometryElement geometryElement = rebarOne.get_Geometry(options);

            //GeometryElement geometryElement = null;
            //if (rebarOne != null)
            //{
            //    geometryElement = rebarOne.GetFullGeometryForView(active_view);

            //    foreach (GeometryObject geometryObject in geometryElement)
            //    {
            //        Solid solid = geometryObject as Solid;
            //        if (solid == null) continue;
            //        if (solid.Edges.Size == 0) continue;
            //        foreach (Edge edge in solid.Edges)
            //        {
            //            ElementId eid = edge.GraphicsStyleId;
            //            return doc.GetElement(eid) as GraphicsStyle;
            //        }
            //    }

            //}

            //if (rebarIn != null)
            //{                  
            //    geometryElement = rebarIn.get_Geometry(options);
            //    IList<Subelement> sub= rebarIn.GetSubelements();

            //    foreach(Subelement s in sub)
            //    {
            //        string name = s.Element.Name;
            //    }

            //    //foreach (GeometryElement geoelement in geometryElement)
            //    //{
            //    //    foreach (GeometryObject geometryObject in geoelement)
            //    //    {
            //    //        Solid solid = geometryObject as Solid;
            //    //        if (solid == null) continue;
            //    //        if (solid.Edges.Size == 0) continue;
            //    //        foreach (Edge edge in solid.Edges)
            //    //        {
            //    //            ElementId eid = edge.GraphicsStyleId;
            //    //            return doc.GetElement(eid) as GraphicsStyle;
            //    //        }
            //    //    }
            //    //}       
        }


        /// <summary>
        /// Создать дополнительные линии для отображения стержня
        /// </summary>
        void CreateOffSetCurve(Curve curve, bool IsMainIndex)
        {   
            if(curve.GetType().Name=="Line")
            {    
                Line line = curve as Line;
                XYZ p1 = line.GetEndPoint(0);
                XYZ p2 = line.GetEndPoint(1);
                XYZ  offset = line.Direction.CrossProduct(active_view.ViewDirection)* Diametr / 2;
                Line line1 = Line.CreateBound(p1 + offset, p2 + offset);
                DetailCurve newcurve = doc.Create.NewDetailCurve(active_view,line1);                
                newcurve.LineStyle = gs_test;
                detailCurves.Add(newcurve);
                Eids.Add(newcurve.Id);
                // ElementTransformUtils.MoveElement(doc, newcurve.Id, line.Direction.CrossProduct(active_view.ViewDirection)*Diametr/2);
                if (IsMainIndex)
                {
                    Transform t = Transform.Identity;
                    t = t.ScaleBasis(1 / scale);
                    MainLineScale = line.CreateTransformed(t) as Line;
                    MainLine = line;

                    //MainDetailCurve = newcurve;  // фиксируем основной элемент на чертеже
                }
 
                    outline.AddPoint(newcurve.GeometryCurve.GetEndPoint(0));
                    outline.AddPoint(newcurve.GeometryCurve.GetEndPoint(0));

                //ICollection<ElementId> eid = ElementTransformUtils.CopyElement(doc, newcurve.Id, line.Direction.CrossProduct(active_view.ViewDirection).Negate()*Diametr);
                //newcurve = doc.GetElement(eid.First()) as DetailCurve;

                Line line2 = Line.CreateBound(p1 - offset, p2 - offset);
                newcurve = doc.Create.NewDetailCurve(active_view, line2);
                newcurve.LineStyle = gs_test;
                detailCurves.Add(newcurve);
                Eids.Add(newcurve.Id);

                // добавим точки к контуру эскиза
                outline.AddPoint(newcurve.GeometryCurve.GetEndPoint(0));
                outline.AddPoint(newcurve.GeometryCurve.GetEndPoint(1));
            }
            if (curve.GetType().Name == "Arc")
            {
                try
                {
                    Arc arc = curve as Arc;                    

                    //DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, curve);
                    //newcurve.LineStyle = gs_test;
                    //detailCurves.Add(newcurve);
                    //Eids.Add(newcurve.Id);
                    XYZ start = (arc.GetEndPoint(0) - arc.Center).Normalize();
                    XYZ end = (arc.GetEndPoint(1) - arc.Center).Normalize();
                    XYZ middle = arc.Tessellate().ElementAt(arc.Tessellate().Count/2);
                    XYZ center = (middle - arc.Center).Normalize();

                    XYZ Astart = arc.Center + start * (arc.Radius + Diametr / 2);
                    XYZ Aend = arc.Center + end * (arc.Radius + Diametr / 2);
                    XYZ Acenter = arc.Center + center * (arc.Radius + Diametr / 2);                    
                    Arc new_arc = Arc.Create(Astart, Aend, Acenter);

                    //outline.AddPoint(Acenter);

                    //// добавляем все точки арки
                    //foreach (XYZ p in new_arc.Tessellate())
                    //{
                    //    outline.AddPoint(p);  // добавим точку на внешний контур
                    //}

                    DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, new_arc);
                    newcurve.LineStyle = gs_test;
                    detailCurves.Add(newcurve);
                    Eids.Add(newcurve.Id);

                    Arc on_drawing = newcurve.GeometryCurve as Arc;
                    // добавляем все точки арки
                    foreach (XYZ p in on_drawing.Tessellate())
                    {
                        outline.AddPoint(p);  // добавим точку на внешний контур
                    }

                    Astart = arc.Center + start * (arc.Radius - Diametr / 2);
                    Aend = arc.Center + end * (arc.Radius - Diametr / 2);
                    Acenter = arc.Center + center * (arc.Radius - Diametr / 2);                    
                    new_arc = Arc.Create(Astart, Aend, Acenter);

                    //outline.AddPoint(Acenter);

                    //// добавляем все точки арки
                    //foreach (XYZ p in new_arc.Tessellate())
                    //{
                    //    outline.AddPoint(p);  // добавим точку на внешний контур
                    //}

                    newcurve = doc.Create.NewDetailCurve(active_view, new_arc);
                    newcurve.LineStyle = gs_test;
                    detailCurves.Add(newcurve);
                    Eids.Add(newcurve.Id);

                    on_drawing = newcurve.GeometryCurve as Arc;
                    // добавляем все точки арки
                    foreach (XYZ p in on_drawing.Tessellate())
                    {
                        outline.AddPoint(p);  // добавим точку на внешний контур
                    }


                }
                catch { }

            }
            return;
        }

        /// <summary>
        /// Создать дополнительные торцевые линии для отображения стержня
        /// </summary>
        void CreateOffSetCurveOnEnds(Curve curve,int i=0)
        {                 
                Line line = curve as Line;
                Arc arc = curve as Arc;
                XYZ p0 = curve.GetEndPoint(i);

            if (line != null)
            {                
                XYZ p1 = p0 + line.Direction.CrossProduct(active_view.ViewDirection) * Diametr / 2;
                XYZ p2 = p0 + line.Direction.CrossProduct(active_view.ViewDirection).Negate() * Diametr / 2;
                if (p1.DistanceTo(p2) > 0.0032) line = Line.CreateBound(p1, p2);
                else return;
            }
            if(arc!=null)
            {
                XYZ dir = (p0 - arc.Center).Normalize();
                XYZ p1 = p0 + dir * Diametr / 2;
                XYZ p2 = p0 - dir * Diametr / 2;
                if (p1.DistanceTo(p2) > 0.0032) line = Line.CreateBound(p1, p2);
                else return;
            }

            if (line != null || arc != null)
            {
                DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, line);
                newcurve.LineStyle = gs_test;
                detailCurves.Add(newcurve);
                Eids.Add(newcurve.Id);
            }
            
            return;
        }
                             
        /// <summary>
        /// Создать текстовую надпись
        /// </summary>
        void CreateTextNote(TextOnRebar label, TextNote tn)
        {
            List<Curve> diagonals = new List<Curve>();

            if (label.angleI != 0)
            {
                // получим ось вращения текста
                Line axes = Line.CreateUnbound(label.position, active_view.ViewDirection);
                tn.Location.Rotate(axes, label.angleI);
            }

            // смещаем середину надписи в точку вставки
            XYZ move_middle = tn.BaseDirection.Negate() * active_view.Scale * tn.Width * 0.5;
            // tn.Location.Move(tn.BaseDirection.Negate() * active_view.Scale * tn.Width * 0.5);
            // все надписи пытаемся вывести наружу от чертежа эскиза
            List<double> dist = new List<double>();
            dist.Add(line_left.Distance(label.position));
            dist.Add(line_right.Distance(label.position));
            dist.Add(line_up.Distance(label.position));
            dist.Add(line_down.Distance(label.position));

            // double dist_min = 0;

            switch (label.angleI)
            {
                case 0:
                    if(dist[2]<dist[3])
                    {
                        // ближе всего к верхнему краю                         
                        tn.Location.Move(move_middle + tn.UpDirection * (active_view.Scale * tn.Height * 1.15 + Diametr));                         
                        diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи
                        // проверим: нет ли пересечений с крюками
                        if (CheckTextPosition(diagonals))
                        {
                            // оставляем надпись внутри чертежа
                            tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 1.3 + Diametr));
                            //tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 1.15 + Diametr));
                            //tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 0.15));
                        }
                    }
                    else
                    {
                        // ближе всего к нижнему краю
                        tn.Location.Move(move_middle + tn.UpDirection.Negate() * active_view.Scale * tn.Height * 0.15);
                        diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи
                        // добавим к внешнему контуру - т.к. метка внизу чертежа
                        outline.AddPoint(diagonals[0].GetEndPoint(0));
                        outline.AddPoint(diagonals[0].GetEndPoint(1));
                        // проверим: нет ли пересечений с крюками
                        if (CheckTextPosition(diagonals))
                        {
                            // оставляем надпись внутри чертежа
                            tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height * 1.3 + Diametr));
                            //tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height * 1.15 + Diametr));
                            //tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height * 0.15));
                        }
                    }
                    break;
                case 1.571:
                    if (dist[0] < dist[1])
                    {
                        // ближе всего к левому краю
                        tn.Location.Move(move_middle + tn.UpDirection * (active_view.Scale * tn.Height + Diametr));
                        diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи
                        // проверим: нет ли пересечений с крюками
                        if (CheckTextPosition(diagonals))
                        {
                            // оставляем надпись внутри чертежа
                            tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 1.15 + Diametr));
                            //tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height + Diametr));
                            //tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 0.15));
                        }
                    }
                    else
                    {
                        // ближе всего к правому краю
                        tn.Location.Move(move_middle + tn.UpDirection.Negate() * active_view.Scale * tn.Height * 0.15);
                        diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи
                        // проверим: нет ли пересечений с крюками
                        if (CheckTextPosition(diagonals))
                        {
                            // оставляем надпись внутри чертежа
                            tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height * 1.15 + Diametr));
                            //tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height + Diametr));
                            //tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height * 0.15));
                        }
                    }
                    break;
                default:
                    if (dist[2] < dist[3])
                    {
                        // ближе всего к верхнему краю
                        tn.Location.Move(move_middle + tn.UpDirection * (active_view.Scale * tn.Height * 1.15 + Diametr));
                        diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи
                        // проверим: нет ли пересечений с крюками
                        if (CheckTextPosition(diagonals))
                        {
                            // оставляем надпись внутри чертежа
                            tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height * 1.30 + Diametr));
                            //tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height*1.15 + Diametr));
                            //tn.Location.Move(tn.UpDirection* (active_view.Scale * tn.Height * 0.15));
                        }
                    }
                    else
                    {
                        // ближе всего к нижнему краю
                        tn.Location.Move(move_middle + tn.UpDirection.Negate() * active_view.Scale * tn.Height * 0.15);
                        diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи
                        // добавим к внешнему контуру - т.к. метка внизу чертежа
                        outline.AddPoint(diagonals[0].GetEndPoint(0));
                        outline.AddPoint(diagonals[0].GetEndPoint(1));
                        outline.AddPoint(diagonals[1].GetEndPoint(0));
                        outline.AddPoint(diagonals[1].GetEndPoint(1));
                        // проверим: нет ли пересечений с крюками
                        if (CheckTextPosition(diagonals))
                        {
                            // оставляем надпись внутри чертежа
                            tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 1.30 + Diametr));
                            //tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 1.15 + Diametr));
                            //tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height * 0.15));
                        }
                    }
                    break;
            }


            //if (dist[0] == dist_min) // ближе всего к левому краю
            //{
            //    if (Math.Abs(label.angleI) <= Math.PI / 4)
            //         tn.Location.Move(tn.BaseDirection.Negate() * active_view.Scale * tn.Width * 0.5);
            //    else tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height + Diametr));
            //}

            //if (dist[1] == dist_min) // ближе всего к правому краю
            //{
            //    if (Math.Abs(label.angleI) <= Math.PI / 4)
            //        tn.Location.Move(tn.BaseDirection * active_view.Scale * tn.Width * 0.5);
            //    else
            //        tn.Location.Move(tn.UpDirection.Negate() * (active_view.Scale * tn.Height*0.1));

            //}

            //if (dist[2] == dist_min) // ближе всего к верхнему краю
            //{
            //    if (Math.Abs(label.angleI) <= Math.PI / 4)
            //        tn.Location.Move(tn.UpDirection * (active_view.Scale * tn.Height * 1.1 + Diametr));                 
            //    else
            //        tn.Location.Move(tn.BaseDirection * active_view.Scale * tn.Width * 0.5);
            //}
            //if (dist[3] == dist_min) // ближе всего к нижнему краю
            //{
            //    if (Math.Abs(label.angleI) <= Math.PI / 4)
            //        tn.Location.Move(tn.UpDirection.Negate() * active_view.Scale * tn.Height * 0.1);                 
            //    else
            //        tn.Location.Move(tn.BaseDirection.Negate() * active_view.Scale * tn.Width * 0.5);
            //}           
        }

        /// <summary>
        /// Создать текстовую надпись на крюках
        /// </summary>
        void CreateTextNoteOnHook(TextOnRebar label, TextNote tn)
        {
            List<Curve> diagonals = new List<Curve>();

            if (label.angleI != 0)
            {
                // получим ось вращения текста
                Line axes = Line.CreateUnbound(label.position, active_view.ViewDirection);
                tn.Location.Rotate(axes, label.angleI);
            }

            // смещаем середину надписи в точку вставки
            tn.Location.Move(tn.BaseDirection.Negate() * active_view.Scale * tn.Width * 0.5);
            tn.Location.Move(tn.UpDirection.Negate() * 3*Diametr/4);
            

            diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи

            //DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, diagonals[0]);
            //Eids.Add(newcurve.Id);
            //newcurve = doc.Create.NewDetailCurve(active_view, diagonals[1]);
            //Eids.Add(newcurve.Id);

            if (!CheckTextPosition(diagonals))
            {
                // добавим к внешнему контуру - т.к. метка внизу чертежа
                outline.AddPoint(diagonals[0].GetEndPoint(0));
                outline.AddPoint(diagonals[0].GetEndPoint(1));
                outline.AddPoint(diagonals[1].GetEndPoint(0));
                outline.AddPoint(diagonals[1].GetEndPoint(1));
                return;
            }

            // перемещаем на другую сторону линии
            tn.Location.Move(tn.UpDirection * (2 * 3 * Diametr / 4 + active_view.Scale * tn.Height));
            diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи

            //DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, diagonals[0]);
            //Eids.Add(newcurve.Id);

            //newcurve = doc.Create.NewDetailCurve(active_view, cross_lines_hooks[5]);
            //Eids.Add(newcurve.Id);


            //newcurve = doc.Create.NewDetailCurve(active_view, diagonals[1]);
            //Eids.Add(newcurve.Id);

            //foreach (Curve curve in cross_lines_hooks)
            //{
            //    newcurve = doc.Create.NewDetailCurve(active_view, curve);
            //    Eids.Add(newcurve.Id);
            //}

            if (!CheckTextPosition(diagonals))
            {
                // добавим к внешнему контуру - т.к. метка внизу чертежа
                outline.AddPoint(diagonals[0].GetEndPoint(0));
                outline.AddPoint(diagonals[0].GetEndPoint(1));
                outline.AddPoint(diagonals[1].GetEndPoint(0));
                outline.AddPoint(diagonals[1].GetEndPoint(1));
                return;
            }

            // на конце линии
            XYZ move_vec = label.start_initial - tn.Coord;
            tn.Location.Move(move_vec);

            if(Math.Round(label.dir_segment.AngleTo(tn.BaseDirection),3)<1.57)
            tn.Location.Move(label.dir_segment * active_view.Scale * tn.Width*0.1);
            else
            tn.Location.Move(label.dir_segment * active_view.Scale * tn.Width);

            tn.Location.Move(tn.UpDirection * active_view.Scale * tn.Height/2);

            diagonals = GetDiagonalsTextNote(tn);   // получить диагонали текстовой надписи
            // добавим к внешнему контуру - т.к. метка внизу чертежа
            outline.AddPoint(diagonals[0].GetEndPoint(0));
            outline.AddPoint(diagonals[0].GetEndPoint(1));
            outline.AddPoint(diagonals[1].GetEndPoint(0));
            outline.AddPoint(diagonals[1].GetEndPoint(1));
        }





        ///// <summary>
        ///// Добавить сопуствующие элементы чертежа
        ///// </summary>
        //void AddOtherDetailCurve(ElementId eid) 
        //{             
        //    double d = Math.Round(Diametr / 2, 3);             
        //    DetailCurve dcurve_axis = doc.GetElement(eid) as DetailCurve;
        //    Curve curve_axis = dcurve_axis.GeometryCurve;

        //    foreach (DetailCurve dcurve_drawing in detailCurves)
        //    {
        //        Curve curve_drawing = dcurve_drawing.GeometryCurve;                
        //        SetComparisonResult scr = curve_axis.Intersect(curve_drawing); 
        //        if(scr.Equals(SetComparisonResult.Disjoint))   // нет пересечения кривых
        //        {
        //            // проверим расстояние
        //            double dist = Math.Min(curve_axis.Distance(curve_drawing.GetEndPoint(0)),
        //                                   curve_axis.Distance(curve_drawing.GetEndPoint(1)));
        //            if (Math.Round(dist, 3) == d) elements_rotate.Add(dcurve_drawing.Id);
        //        }
        //    }            
        //}
        ///// <summary>
        ///// Добавить закрывающие элементы
        ///// </summary>
        //void AddOtherDetailCurveEnd(ElementId eid)
        //{    
        //    double D = Math.Round(Diametr, 3);
        //    DetailCurve dcurve_axis = doc.GetElement(eid) as DetailCurve;
        //    Curve curve_axis = dcurve_axis.GeometryCurve;

        //    foreach (DetailCurve dcurve_drawing in detailCurves)
        //    {
        //        Curve curve_drawing = dcurve_drawing.GeometryCurve;
        //        if (Math.Round(curve_drawing.Length, 3) == D)
        //        {
        //            // проверим расстояние
        //            double dist = Math.Min(curve_axis.Distance(curve_drawing.GetEndPoint(0)),
        //                                   curve_axis.Distance(curve_drawing.GetEndPoint(1)));
        //            if (Math.Round(dist, 3) == D)
        //            {
        //                elements_rotate.Add(dcurve_drawing.Id);
        //                return;
        //            }                    
        //        }                 
        //    }             
        //}



        /// <summary>
        /// Получить новые координаты стержня c учетом возможного разворачивания хомутов
        /// </summary>
        void TransformRebarStirrup(Autodesk.Revit.DB.View view)
        {
            // развертывание хомутов выполняется в плоскости стержня
            // используется текущий вид (если стержень расположен на нем) или временный вид
            if (!IsOpenStirrup()) return; // нет необходимости раскрывать хомут

            int count = ilc_transform.Count;
            // контрольные параметры для определения стороны поворота
           
            // построим временные линии и повернем их             
            DetailCurve newcurve1 = doc.Create.NewDetailCurve(view, ilc_transform[count - 1]);
            elements_rotate.Add(newcurve1.Id);
            DetailCurve newcurve2 = doc.Create.NewDetailCurve(view, ilc_transform[count - 2]);
            elements_rotate.Add(newcurve2.Id);
            DetailCurve newcurve3 = doc.Create.NewDetailCurve(view, ilc_transform[count - 3]);
            elements_rotate.Add(newcurve3.Id);
            DetailCurve newcurve4 = doc.Create.NewDetailCurve(view, ilc_transform[count - 4]);
            DetailCurve newcurve5 = doc.Create.NewDetailCurve(view, ilc_transform[count - 5]);

            //XYZ control_point = (ilc_transform[count - 5].GetEndPoint(0) + ilc_transform[count - 5].GetEndPoint(1)) / 2;
            //double contol_dist = ilc_transform[count - 3].GetEndPoint(1).DistanceTo(control_point);
            //Line axis_rotate = Line.CreateUnbound(ilc_transform[count - 4].GetEndPoint(1), ZRebarNormal); // ось вращения

            XYZ control_point = (newcurve5.GeometryCurve.GetEndPoint(0) + newcurve5.GeometryCurve.GetEndPoint(1)) / 2;
            double contol_dist = newcurve3.GeometryCurve.GetEndPoint(1).DistanceTo(control_point);
            Line axis_rotate = Line.CreateUnbound(newcurve4.GeometryCurve.GetEndPoint(1), ZRebarNormal); // ось вращения


            //for(int i=0; i<count -3;i++)
            //{
            //    newcurve1 = doc.Create.NewDetailCurve(view, ilc_transform[i]);
            //}


            ElementTransformUtils.RotateElements(doc, elements_rotate, axis_rotate, 20 * Math.PI / 180);

            doc.Regenerate();
            // DetailCurve dc = doc.GetElement(elements_rotate.First()) as DetailCurve;
            Line line = newcurve3.GeometryCurve as Line;
            double contol_dist2 = line.GetEndPoint(1).DistanceTo(control_point);

            if (contol_dist2 < contol_dist)  // тогда в другую сторону
            {
                ElementTransformUtils.RotateElements(doc, elements_rotate, axis_rotate, -40 * Math.PI / 180);

            }

            // запишем измененное состояние
            ilc_transform[count - 1] = newcurve1.GeometryCurve;
            ilc_transform[count - 2] = newcurve2.GeometryCurve;
            ilc_transform[count - 3] = newcurve3.GeometryCurve;

            elements_rotate.Add(newcurve4.Id);
            elements_rotate.Add(newcurve5.Id);

            doc.Delete(elements_rotate);  // удалим временные линии
            elements_rotate.Clear();
        }

        /// <summary>
        /// Проверка необходимости разворачивания формы стержня
        /// </summary>

        bool IsOpenStirrup()
        {
            bool cross = false;
                                       
            if (hook_start > 0 && hook_end > 0 && !IsHermiteSpline)
            {                
                // зона 1 крюка
                hook_start_outline = new Outline(ilc[0].GetEndPoint(0), ilc[0].GetEndPoint(1));
                hook_start_outline.AddPoint(ilc[1].GetEndPoint(1));
                int count = ilc_transform.Count;
                // зона 2 крюка
                hook_end_outline = new Outline(ilc[count - 1].GetEndPoint(0), ilc[count - 1].GetEndPoint(1));
                hook_end_outline.AddPoint(ilc[count - 2].GetEndPoint(0));

                // если крюки пересекаются
                if (hook_start_outline.Intersects(hook_end_outline, 0.001))
                {
                    cross = true;
                }
                // если крюки близко расположены (менее 10 см)
                if (ilc[0].GetEndPoint(0).DistanceTo(ilc[count - 1].GetEndPoint(1)) < 0.328084)
                {
                    cross = true;
                }

                // если прямые сегменты накладываются друг на друга
                SetComparisonResult scr = ilc[2].Intersect(ilc[count - 3]);
                if (scr.Equals(SetComparisonResult.Overlap))
                {
                    cross = true;
                }
            }
            return cross;

        }

        /// <summary>
        /// Получить координаты стержня в системе координат детального вида
        /// </summary>
        bool TransformToNewCoordinateSystem()
        {
            // Масштабирование эскиза
            Transform t = Transform.Identity;
            t = t.ScaleBasis(scale);

            ilc_transform.Clear();

            if (IsRebarOnViewPlane)
            {
                if (!IsHermiteSpline)
                {   
                    // основной случай - вектор нормали стержня ортогонален текущему виду
                    // координаты не трансформируем - рисуем как есть
                    foreach (Curve c in ilc)
                    {                       
                        Curve newcurve= c.CreateTransformed(t);
                        ilc_transform.Add(newcurve);
                        ilc_transform_initial.Add(newcurve);
                    }
                    // выполним проверку необходимости раскрытия хомута и изменим положение сегментов
                    // все выполняем на текущем виде
                    TransformRebarStirrup(active_view);
                    return true;
                }
                else
                {   
                    for (int i = 0; i < ilc.Count; i++)
                    {
                        Curve c = ilc[i];                      
                           
                                IList<XYZ> points = c.Tessellate();
                                for (int j = 0; j < points.Count - 1; j++)
                                {                            
                                    XYZ p1 = SketchTools.ProjectPointOnWorkPlane(plane3D, points[j]);
                                    XYZ p2 = SketchTools.ProjectPointOnWorkPlane(plane3D, points[j+1]);
                                        try
                                        {
                                            Line line = Line.CreateBound(p1, p2);
                                            ilc_transform.Add(line);
                                        }
                                        catch { }
                                }                           
                    }
                }
                
            }
            else
            {                 
                CreateTempViewDetail();
                // Autodesk.Revit.DB.ViewSection vs = CreateTempViewDetail();              // готовим временный вид для стержня
                // координаты стержня формируем на временном виде
                if (SketchCommand.vs == null) return false;
                // выполним проверку необходимости раскрытия хомута и изменим положение сегментов
                // все выполняем на временном виде
                TransformRebarStirrup(SketchCommand.vs);
                Transform ts = ElementTransformUtils.GetTransformFromViewToView(SketchCommand.vs, active_view);
                ts = ts.ScaleBasis(scale);
                // выполним трансформацию линий для текущего вида
                for (int i=0; i<ilc_transform.Count; i++)
                {                     
                    ilc_transform[i]= ilc_transform[i].CreateTransformed(ts);              // получить кривые в системе координат вида
                    ilc_transform_initial[i] = ilc_transform_initial[i].CreateTransformed(ts);
                }
                // doc.Delete(vs.Id);                                                         // временный вид удаляем                
            }

            return true;
        }

        /// <summary>
        /// Получить высоту текста на текущем виде
        /// </summary>
        double GetHeightText(TextNote tn)
        {
            XYZ p1 = tn.get_BoundingBox(active_view).Max;
            XYZ p2 = tn.get_BoundingBox(active_view).Min;
            XYZ vec = (p2 - p1);
            XYZ vecH = tn.UpDirection;
            double angle = Math.Abs(vec.AngleTo(vecH));
            if (angle > Math.PI / 2) angle = Math.PI - angle;
            return vec.GetLength() * Math.Cos(angle)/2;
        }
        /// <summary>
        /// Получить ширину текста на текущем виде
        /// </summary>
        double GetWidthText(TextNote tn)
        {
            XYZ p1 = tn.get_BoundingBox(active_view).Max;
            XYZ p2 = tn.get_BoundingBox(active_view).Min;
            XYZ vec = (p2 - p1);
            XYZ vecH = tn.BaseDirection;
            double angle = Math.Abs(vec.AngleTo(vecH));
            if (angle > Math.PI / 2) angle = Math.PI - angle;
            return vec.GetLength() * Math.Cos(angle);
        }

        /// <summary>
        /// Получить диагонали текстовой области
        /// </summary>
        /// <param name="tn">Текстовая надпись</param>         
        /// <returns>Две диагонали текстовой области</returns>
        List<Curve> GetDiagonalsTextNote(TextNote tn)
        {
            List<Curve> line = new List<Curve>();
            double W = active_view.Scale * tn.Width;
            double H = active_view.Scale * tn.Height;
            XYZ p1 = tn.Coord;
            XYZ p2 = tn.Coord + tn.BaseDirection * W;
            XYZ p3 = p2+ tn.UpDirection.Negate() * H;
            XYZ p4 = tn.Coord + tn.UpDirection.Negate() * H;
            p1 = SketchTools.ProjectPointOnWorkPlane(work_plane_drawing.p1, work_plane_drawing.p2, work_plane_drawing.p3, p1);
            p2 = SketchTools.ProjectPointOnWorkPlane(work_plane_drawing, p2);
            p3 = SketchTools.ProjectPointOnWorkPlane(work_plane_drawing, p3);
            p4 = SketchTools.ProjectPointOnWorkPlane(work_plane_drawing, p4);
            Curve line1 = Line.CreateBound(p1, p3);
            Curve line2 = Line.CreateBound(p2, p4);
            line.Add(line1);
            line.Add(line2);
            return line;            
        }

        /// <summary>
        /// Проверить пересечение текстовой надписи для крюков
        /// </summary>
        /// <param name="tn">Текстовая надпись</param>
        /// <returns>Признак пересечения</returns>              

        bool CheckTextPosition(List<Curve> text_lines)
        {
            IntersectionResultArray ira = null;
            foreach(Curve text in text_lines )
            {
                foreach (Curve sketch in cross_lines_hooks)
                {
                    text.Intersect(sketch, out ira);
                    if (ira == null) continue;
                    if (ira.get_Item(0).XYZPoint != null) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Получить проекцию точки на плоскость, заданную тремя точками
        /// </summary>
        /// <param name="PointPlane1">Точка 1 плоскости</param>
        /// <param name="PointPlane2">Точка 2 плоскости</param>
        /// <param name="PointPlane3">Точка 3 плоскости</param>
        /// <param name="p">Проекцируемая точка</param>         
        /// <returns>Точка проекции</returns>
        public static XYZ ProjectPointOnWorkPlane(XYZ PointPlane1, XYZ PointPlane2, XYZ PointPlane3, XYZ p)
        {


            XYZ a = PointPlane1 - PointPlane2;
            XYZ b = PointPlane1 - PointPlane3;
            XYZ c = p - PointPlane1;



            XYZ normal = (a.CrossProduct(b));

            try
            {
                normal = normal.Normalize();
            }
            catch (Exception)
            {
                normal = XYZ.Zero;
            }

            XYZ retProjectedPoint = p - (normal.DotProduct(c)) * normal;
            return retProjectedPoint;

        }

        /// <summary>
        /// Получить направление и координату на виде для основного сегмента стержня
        /// </summary>
        void GetDirMainSegment()
        {
            if (IsRebarOnViewPlane) // если стержень не лежит в плоскости чертежа
            {
                if (rsds != null) // сегментный стержень
                {
                    int main_segment = rsds.MajorSegmentIndex;  // нумерация прямых сегментов только
                    if (main_segment < 0) main_segment = 0;                                        
                    if (ilc_lines.Count > main_segment)
                    {
                        DirMainSegment = (ilc_lines[main_segment].GetEndPoint(1) - ilc_lines[main_segment].GetEndPoint(0)).Normalize();
                        ProjectMainSegmentOnView = (ilc_lines[main_segment].GetEndPoint(1) + ilc_lines[main_segment].GetEndPoint(0)) / 2;
                        // приведем значение к общему списку кривых
                        IndexMainSegment = main_segment * 2;
                        // if (IndexMainSegment < 0) IndexMainSegment = 0;
                        if (hook_start > 0) IndexMainSegment = IndexMainSegment + 2;
                    }
                }                
            }
            else   // для стержня вне плоскости чертежа
            {
                // основным сегментом назначаем сегмент, лежащий в плоскости чертежа
                for (int i=0; i<ilc.Count; i++)
                {
                    XYZ p0= ilc[i].GetEndPoint(0);
                    XYZ p1= ilc[i].GetEndPoint(1);
                    XYZ vector = (p1 - p0).Normalize();
                    if( Math.Round(vector.AngleTo(active_view.ViewDirection),3) == Math.Round(Math.PI/2,3))
                    {
                        DirMainSegment = vector;
                        ProjectMainSegmentOnView = (p0+p1) / 2;
                        IndexMainSegment = i;
                    }
                }
                // если не обнаружено ни одного сегмента в плоскости чертежа
                if(DirMainSegment==null)
                {
                    double dist = 0;
                    // основным сегментом назначаем сегмент, имеющий самую длинную проекцию
                    for (int i = 0; i < ilc.Count; i++)
                    {
                        XYZ p0 = ilc[i].GetEndPoint(0);
                        XYZ p1 = ilc[i].GetEndPoint(1);
                        XYZ vector = (p1 - p0).Normalize();
                        XYZ p0_prj = SketchTools.ProjectPointOnWorkPlane(plane3D, p0);
                        XYZ p1_prj = SketchTools.ProjectPointOnWorkPlane(plane3D, p1);
                        double dist_current = Math.Round(p0_prj.DistanceTo(p1_prj));
                        if (dist<dist_current)                         
                        {
                            dist = dist_current;
                            DirMainSegment = (p1_prj - p0_prj).Normalize();
                            ProjectMainSegmentOnView = (p1_prj + p0_prj) / 2;
                            IndexMainSegment = i;
                        }
                    }
                }
                // если не обнаружено ни одного сегмента в плоскости чертежа- вероятно просто точка
                if (DirMainSegment == null)
                {
                    DirMainSegment = active_view.ViewDirection;
                    XYZ p0 = ilc[0].GetEndPoint(0);
                    ProjectMainSegmentOnView = SketchTools.ProjectPointOnWorkPlane(plane3D, p0);
                    IndexMainSegment = 0;
                }


                }

            if (ProjectMainSegmentOnView == null)
            {
                // точка основного сегмента на виде
                BoundingBoxXYZ bb = rebar.get_BoundingBox(active_view);
                ProjectMainSegmentOnView = (bb.Max + bb.Min) / 2;
            }
            // проекция точки на базовый вид
            ProjectMainSegmentOnView = SketchTools.ProjectPointOnWorkPlane(plane3D, ProjectMainSegmentOnView);
        }

        /// <summary>
        /// Создание временного детального вида для подготовки эскизов
        /// </summary>
        // ViewSection CreateTempViewDetail()
        void CreateTempViewDetail()
        {
            Matrix4 MatrixMain_arc = null;
            // Transform transform = Transform.Identity;
            temp_view.Origin = ilc[0].GetEndPoint(0);
            temp_view.BasisZ = ZRebarNormal;  // плоcкость стержня

            // ось Х принимаем по основному сегменту стержня            
            if(rsds != null) // сегментный стержень
            {
                //int main_segment = rsds.MajorSegmentIndex;
                //if (main_segment < 0) main_segment = 0;
                //if (hook_start > 0) main_segment = main_segment + 2;
                //if (ilc.Count < main_segment) return null;
                // transform.BasisX = (ilc[main_segment].GetEndPoint(1) - ilc[main_segment].GetEndPoint(0)).Normalize();                
                if (DirMainSegment == null) { SketchCommand.vs = null; return; }
                temp_view.BasisX = DirMainSegment;
                //temp_view.Origin = new XYZ(0, 0, 0);
                //temp_view.BasisX = new XYZ(0, 1, 0);
                //temp_view.BasisZ = new XYZ(1, 0, 1);
                //temp_view.BasisZ = temp_view.BasisZ.Normalize();
                temp_view.BasisY = temp_view.BasisZ.CrossProduct(temp_view.BasisX).Normalize();                 
            }

            if(rarc!=null)
            {
                if (rarc.Type == RebarShapeDefinitionByArcType.Spiral)
                {


                    // получить матрицу преобразований координат: из общей системы в локальную систему стержня                
                    // начало системы координат принимаем в произвольной точке стержня 
                    Vector4 origin_arc = new Vector4(ilc[0].GetEndPoint(0));
                    // направление оси Х 
                    Vector4 xAxis_arc = new Vector4((ilc[0].GetEndPoint(1) - ilc[0].GetEndPoint(0)));
                    xAxis_arc.Normalize();
                    // направление оси Y стены
                    Vector4 yAxis_arc = new Vector4(XYZ.Zero);
                    yAxis_arc = Vector4.CrossProduct(xAxis_arc, zAxis);
                    yAxis_arc.Normalize();

                    MatrixMain_arc = new Matrix4(zAxis, yAxis_arc, xAxis_arc, origin_arc);
                    MatrixMain_arc = MatrixMain_arc.Inverse();

                    temp_view = Transform.Identity;  // оставляем базовую систему
                    temp_view.Origin = ilc[0].GetEndPoint(0);
                                                                          

                    //temp_view.BasisX = ZRebarNormal;
                    //// найти ненулевую координату
                    //if (ZRebarNormal.X!=0)
                    //{
                    //    double bx = -(ZRebarNormal.Y + ZRebarNormal.Z) / ZRebarNormal.X;
                    //    temp_view.BasisZ = new XYZ(bx, 1, 1).Normalize();                       
                    //    goto BaseY;
                    //}
                    //if (ZRebarNormal.Y != 0)
                    //{
                    //    double by = -(ZRebarNormal.X + ZRebarNormal.Z) / ZRebarNormal.Y;
                    //    temp_view.BasisZ = new XYZ(1, by, 1).Normalize();
                    //    goto BaseY;
                    //}
                    //if (ZRebarNormal.Z != 0)
                    //{
                    //    double bz = -(ZRebarNormal.X + ZRebarNormal.Y) / ZRebarNormal.Z;
                    //    temp_view.BasisZ = new XYZ(1, 1, bz).Normalize();
                    //    goto BaseY;
                    //}
                }
                else
                {
                    IList<XYZ> tp = ilc[0].Tessellate();
                    temp_view.BasisX = (ilc[0].GetEndPoint(1) - ilc[0].GetEndPoint(0)).Normalize();
                    temp_view.BasisY = temp_view.BasisZ.CrossProduct(temp_view.BasisX);
                }
            }
                       
            BoundingBoxXYZ m_box = new BoundingBoxXYZ();
            m_box.Enabled = true;

            try
            {
                m_box.Transform = temp_view;
            }
            catch
            {
                MessageBox.Show(SketchFull.Resourses.Strings.Texts.Info10,
                                SketchFull.Resourses.Strings.Texts.Attension);
                { SketchCommand.vs = null; return; }
            }

            ElementId DetailViewId = new ElementId(-1);
            if (SketchCommand.vs == null)
            {
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

                SketchCommand.vs = ViewSection.CreateDetail(doc, DetailViewId, m_box);
            }
            if (null == SketchCommand.vs)
            {
               return; 
            }
            //Transform tt = Transform.Identity;
            //tt.BasisX = section.RightDirection;
            //tt.BasisY = section.UpDirection;
            //tt.BasisZ = section.ViewDirection;

            //// получить кривые на временном виде
            //foreach (Curve c in ilc)
            //{
            //    ilc_transform.Add(c.CreateTransformed(tt));   
            //}

            //Vector4 origin = new Vector4(ilc[0].GetEndPoint(0));
            //Vector4 xAxis = new Vector4(temp_view.BasisX);
            //Vector4 yAxis = new Vector4(temp_view.BasisY);
            //Vector4 zAxis = new Vector4(temp_view.BasisZ);

            //Matrix4 matrix4 = new Matrix4(xAxis, yAxis, zAxis, origin);

            //matrix4 = matrix4.Inverse();


            //// определяем рабочую плоскость по трем точкам
            //XYZ planeP1G = section.Origin;
            //XYZ planeP2G = section.Origin + section.RightDirection;
            //XYZ planeP3G = section.Origin + section.UpDirection;

            //Plane3D plane_section = new Plane3D(planeP1G, planeP2G, planeP3G);
            
            
            ilc_transform.Clear();
            ilc_transform_initial.Clear();
            Line fict = Line.CreateBound(SketchCommand.vs.Origin, SketchCommand.vs.Origin + SketchCommand.vs.RightDirection);
            DetailCurve df = doc.Create.NewDetailCurve(SketchCommand.vs, fict);

            for (int i=0; i<ilc.Count; i++)
            {
                Curve c = ilc[i];              
                try
                {
                    if (IsHermiteSpline) // (c.GetType().Name == "HermiteSpline")
                    {
                        //IsHermiteSpline = true;
                        //c = CreateSpiralCurves(c, p_old,plane_section, out p_old);
                        // Curve curve1 = c.CreateTransformed(temp_view);
                        IList<XYZ> points = c.Tessellate();
                        for (int j=0; j<points.Count-1;j++)
                        {
                            Vector4 p1V = MatrixMain_arc.Transform(new Vector4(points[j]));
                            Vector4 p2V = MatrixMain_arc.Transform(new Vector4(points[j+1]));
                            XYZ p1 = new XYZ(p1V.X,p1V.Y,temp_view.Origin.Z);
                            XYZ p2 = new XYZ(p2V.X, p2V.Y, temp_view.Origin.Z);
                            //double dist = p1.DistanceTo(p2)*304.8;
                            //if (dist < 10) continue;
                            try
                            {
                                Line line = Line.CreateBound(p1, p2);
                                DetailCurve dcH = doc.Create.NewDetailCurve(SketchCommand.vs, line);
                                Curve curveH = dcH.GeometryCurve;
                                ilc_transform.Add(curveH);
                                ilc_transform_initial.Add(curveH);
                            }
                            catch { }
                        }
                        continue;
                    }
                    DetailCurve dc = doc.Create.NewDetailCurve(SketchCommand.vs, c);
                    Curve curve = dc.GeometryCurve;
                    ilc_transform.Add(curve);
                    ilc_transform_initial.Add(curve);
                }
                catch { }
            }

            //foreach (Curve c in ilc)
            //{
            //    IList<XYZ> tp = c.Tessellate();
            //    // for (int i = 0; i < tp.Count - 1; i++)
            //    for (int i = 0; i < 1; i++)
            //    {
            //        XYZ p1 = tp[i];
            //        XYZ p2 = tp[i + 1];
            //        Vector4 p_new1 = matrix4.Transform(new Vector4(p1));                      // получить точку в локальной системе координат                                                                          
            //        Vector4 p_new2 = matrix4.Transform(new Vector4(p2));                      // получить точку в локальной системе координат
            //        Curve curve = Line.CreateBound(new XYZ(p_new1.X, p_new1.Y, p_new1.Z),
            //                                       new XYZ(p_new2.X, p_new2.Y, p_new2.Z));
            //        try
            //        {
            //            DetailCurve dc= doc.Create.NewDetailCurve(section, c);

            //        }
            //        catch { }

            //    }
            //}


            // CreateTestLines(DetailViewId);

            return;
        }

        Curve CreateSpiralCurves(Curve curve, XYZ p_old, Plane3D plane, out XYZ P)
        {
            P = p_old;
                IList<XYZ> points = curve.Tessellate();
                Outline outlineHS = new Outline(points[0], points[1]);

                foreach (XYZ p in points)
                {
                    outlineHS.AddPoint(p);
                }
                XYZ p1 = SketchTools.ProjectPointOnWorkPlane(plane, outlineHS.MaximumPoint);
                XYZ p2 = SketchTools.ProjectPointOnWorkPlane(plane, outlineHS.MinimumPoint);
                Line line = Line.CreateBound(p1, p2);
                //if (P != null)
                //{
                //    line = Line.CreateBound(p1, P);                     
                //}
                //P = p2;
            return line as Curve;
        }

        void CreateTestLines(ElementId DetailViewId)
        {

            // ModelLine modelline = doc.GetElement(new ElementId(256115)) as ModelLine;
            // ModelLine modelline = doc.GetElement(new ElementId(532533)) as ModelLine;
            ModelLine modelline = doc.GetElement(new ElementId(532898)) as ModelLine;

            Curve curve1 = modelline.GeometryCurve;
            XYZ old_p1 = curve1.GetEndPoint(0);
            XYZ old_p2 = curve1.GetEndPoint(1);

            // Южный вид 
            Transform tsZ = Transform.Identity;
            tsZ.BasisX = new XYZ(1, 0, 0);
            tsZ.BasisY = new XYZ(0, 0, 1);
            tsZ.BasisZ = new XYZ(0, -1, 0);

            Curve curve = curve1.CreateTransformed(tsZ);
            XYZ Z1 = curve.GetEndPoint(0);
            XYZ Z2 = curve.GetEndPoint(1);

            XYZ Z3 = tsZ.OfPoint(old_p1);
            XYZ Z4 = tsZ.OfPoint(old_p2);

            BoundingBoxXYZ m_boxZ = new BoundingBoxXYZ();
            m_boxZ.Enabled = true;
            m_boxZ.Transform = tsZ;
            ViewSection sectionZ = ViewSection.CreateDetail(doc, DetailViewId, m_boxZ);
            doc.Create.NewDetailCurve(sectionZ, curve);

            // Cеверный вид
            Transform tsN = Transform.Identity;
            tsN.BasisX = new XYZ(-1, 0, 0);
            tsN.BasisY = new XYZ(0, 0, 1);
            tsN.BasisZ = new XYZ(0, 1, 0);


            curve = curve1.CreateTransformed(tsN);

            XYZ N1 = curve.GetEndPoint(0);
            XYZ N2 = curve.GetEndPoint(1);

            XYZ N3 = tsN.OfPoint(old_p1);
            XYZ N4 = tsN.OfPoint(old_p2);

            BoundingBoxXYZ m_boxN = new BoundingBoxXYZ();
            m_boxN.Enabled = true;
            m_boxN.Transform = tsN;
            ViewSection sectionN = ViewSection.CreateDetail(doc, DetailViewId, m_boxN);
            doc.Create.NewDetailCurve(sectionN, curve);

            // Западный вид
            Transform tsW = Transform.Identity;
            tsW.BasisX = new XYZ(0, -1, 0);
            tsW.BasisY = new XYZ(0, 0, -1);
            tsW.BasisZ = new XYZ(1, 0, 0);   // +1 ????

            curve = curve1.CreateTransformed(tsW);

            XYZ W1 = curve.GetEndPoint(0);
            XYZ W2 = curve.GetEndPoint(1);

            XYZ W3 = tsW.OfPoint(old_p1);
            XYZ W4 = tsW.OfPoint(old_p2);

            BoundingBoxXYZ m_box = new BoundingBoxXYZ();
            m_box.Enabled = true;
            m_box.Transform = tsW;
            ViewSection section = ViewSection.CreateDetail(doc, DetailViewId, m_box);
            doc.Create.NewDetailCurve(section, curve);

            // Восточный вид
            Transform tsE = Transform.Identity;
            tsE.BasisX = new XYZ(0, -1, 0);
            tsE.BasisY = new XYZ(0, 0, 1);
            tsE.BasisZ = new XYZ(-1, 0, 0);

            curve = curve1.CreateTransformed(tsE);

            XYZ E1 = curve.GetEndPoint(0);
            XYZ E2 = curve.GetEndPoint(1);

            XYZ E3 = tsE.OfPoint(old_p1);
            XYZ E4 = tsE.OfPoint(old_p2);

            m_box = new BoundingBoxXYZ();
            m_box.Enabled = true;
            m_box.Transform = tsE;
            section = ViewSection.CreateDetail(doc, DetailViewId, m_box);
            doc.Create.NewDetailCurve(section, curve);
        }

        //public void UpdateImage()
        //{
        //    // DivideByTypeRebar();                           // разделить по типам стержней, получить кривые и вектор Z
        //    PreparedDataSegements();
        //    GetInfoAboutHooks();                           // получить данные по крюкам
        //    ChangeParametersRebar();                       // изменить параметры стержня
        //    if (status)
        //    {
        //        IsRebarCorrect = InitialDataForSegments();       // инициализация данных для сегментов
        //        if (IsRebarCorrect)
        //        {
        //            GetPointsAndLinesForDrawing();             // Инициализация данных для сегментов, радиусов загиба 
        //            DrawPicture();
        //        }

        //    }
        //}

        ///// <summary>
        ///// Получить параметры для дугового сегмента
        ///// </summary>
        ///// <param name="curves">Линия стержня</param>
        ///// <param name="rebar">Элемент стержня</param>
        ///// <param name="i">Текущий номер линии</param>
        ///// <param name="value">Значение параметра</param>
        ///// <returns>Текстовая надпись</returns> 
        //TextOnArc GetArcSegment(IList<Curve> curves, Element rebar, int i, double value = 0)
        //{
        //    // получить диаметр стержня
        //    double d = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
        //    TextOnArc toa = new TextOnArc();
        //    toa.rebar = rebar;
        //    Arc arc = curves[i] as Arc;
        //    // запишем координаты, направление сегмента и радиус
        //    toa.position = arc.Center;                                         // запишем координаты центра дуги 
        //    toa.start = (arc.GetEndPoint(0) + arc.GetEndPoint(1)) / 2;         // начальная точка сегмента
        //    toa.end = arc.Center;                                              // конечная точка сегмента    
        //    if (value == 0) toa.value = arc.Radius - d / 2;                    // запишем радиус дуги (по внутреннему контуру)
        //    else toa.value = value;
        //    // получить длину примыкающих прямых сегментов
        //    double l1, l2;
        //    l1 = l2 = 0;
        //    if ((i - 1) >= 0) l1 = curves[i - 1].Length;
        //    if ((i + 1) < curves.Count) l2 = curves[i + 1].Length;
        //    toa.nearestL = l1 + l2;
        //    return toa;
        //}




        /// <summary>
        /// Создать чертеж
        /// </summary>
        //void DrawPicture()
        //{
        //    // готовим рисунок
        //    flag = new Bitmap(sizeX, sizeY);

        //    #region Получим точки с учетом масштаба
        //    for (int i = 0; i < line2D_L.Count; i++)
        //    {
        //        line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X * scale, line2D_L[i].p1F.Y * scale), new PointF(line2D_L[i].p2F.X * scale, line2D_L[i].p2F.Y * scale));
        //    }

        //    for (int i = 0; i < line2D.Count; i++)
        //    {
        //        line2D[i] = new Line2D(new PointF(line2D[i].p1F.X * scale, line2D[i].p1F.Y * scale), new PointF(line2D[i].p2F.X * scale, line2D[i].p2F.Y * scale));
        //    }

        //    for (int i = 0; i < pointDF.Count; i++)
        //    {
        //        pointDF[i] = new PointF(pointDF[i].X * scale, pointDF[i].Y * scale);
        //    }
        //    foreach (TextOnRebar tor in lg) { tor.UsingScale(scale); }
        //    foreach (TextOnArc tor in lg_arc) { tor.UsingScale(scale); }
        //    foreach (TextOnRebar tor in hooks) { tor.UsingScale(scale); }

        //    #endregion Получим точки с учетом масштаба
        //    SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

        //    moveX = (sizeX - 2 * canva - maxX) / 2;
        //    moveY = (sizeY - 2 * canva - maxY) / 2;

        //    //graphic = Graphics.FromImage(flag);
        //    //graphic.Clear(System.Drawing.Color.White);
        //    // отсортируем список по длине примыкающих прямых сегментов            
        //    lg_arc_sorted = lg_arc.OrderByDescending(x => x.nearestL).ToList();
        //}
        /// <summary>
        /// Инициализация данных для сегментов, радиусов загиба
        /// </summary>
        //void GetPointsAndLinesForDrawing()
        //{
        //    float dX = 0;           // смещение центра рисунка
        //    float dY = 0;
        //    // приведем пространственную систему координат стержня в плоскую систему
        //    // получить матрицу преобразований координат: из общей системы в локальную систему стержня                
        //    // начало системы координат принимаем в произвольной точке стержня 

        //    Vector4 origin = new Vector4(p_initial);
        //    // направление оси Х          
        //    Vector4 xAxis = new Vector4(dir_major);
        //    xAxis.Normalize();
        //    // направление оси Y 
        //    Vector4 yAxis = new Vector4(XYZ.Zero);
        //    yAxis = Vector4.CrossProduct(xAxis, zAxis);
        //    yAxis.Normalize();

        //    Matrix4 MatrixMain = new Matrix4(xAxis, yAxis, zAxis, origin);
        //    // после выполнения инверсии в TRANSFORM можем подставлять ГЛОБАЛЬНЫЕ КООРДИНАТЫ и получать ЛОКАЛЬНЫЕ
        //    MatrixMain = MatrixMain.Inverse();
        //    pointDF.Clear();
        //    line2D.Clear();
        //    line2D_L.Clear();
        //    // выполним расчет точек для чертежа линий арматуры
        //    foreach (Curve c in ilc)
        //    {
        //        IList<XYZ> tp = c.Tessellate();
        //        foreach (XYZ p in tp)
        //        {
        //            Vector4 p_new1 = MatrixMain.Transform(new Vector4(p));                      // получить точку в локальной системе координат
        //            PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
        //            pointDF.Add(p_new1F);                                                        // получить точку для картинки                     
        //        }

        //        tp = c.Tessellate();
        //        // получим линии чертежа арматуры
        //        for (int i = 0; i < tp.Count - 1; i++)
        //        {
        //            XYZ p1 = tp[i];
        //            XYZ p2 = tp[i + 1];
        //            Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
        //            PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
        //            Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
        //            PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
        //            Line2D line = new Line2D(p_new1F, p_new2F);
        //            line2D.Add(line);                                                            // добавить линию к списку

        //        }

        //        if (c.GetType().Name == "Arc") continue;                                        // для участка типа дуга

        //        tp = c.Tessellate();
        //        // получим линии чертежа арматуры
        //        for (int i = 0; i < tp.Count - 1; i++)
        //        {
        //            XYZ p1 = tp[i];
        //            XYZ p2 = tp[i + 1];
        //            Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
        //            PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
        //            Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
        //            PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
        //            Line2D line = new Line2D(p_new1F, p_new2F);
        //            line2D_L.Add(line);                                                            // добавить линию к списку

        //        }

        //    }

        //    pointDF = pointDF.ToList();

        //    SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);
        //    // все точки должны быть в 1 четверти
        //    if (minX < 0)
        //        for (int i = 0; i < pointDF.Count(); i++)
        //        {
        //            pointDF[i] = new PointF(pointDF[i].X - minX, pointDF[i].Y);
        //            dX = minX;
        //        }
        //    if (minY < 0)
        //        for (int i = 0; i < pointDF.Count(); i++)
        //        {
        //            pointDF[i] = new PointF(pointDF[i].X, pointDF[i].Y - minY);
        //            dY = minY;
        //        }

        //    if (minX < 0)
        //        for (int i = 0; i < line2D_L.Count(); i++)
        //        {
        //            line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X - minX, line2D_L[i].p1F.Y), new PointF(line2D_L[i].p2F.X - minX, line2D_L[i].p2F.Y));
        //        }
        //    if (minY < 0)
        //        for (int i = 0; i < line2D_L.Count(); i++)
        //        {
        //            line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X, line2D_L[i].p1F.Y - minY), new PointF(line2D_L[i].p2F.X, line2D_L[i].p2F.Y - minY));

        //        }

        //    if (minX < 0)
        //        for (int i = 0; i < line2D.Count(); i++)
        //        {
        //            line2D[i] = new Line2D(new PointF(line2D[i].p1F.X - minX, line2D[i].p1F.Y), new PointF(line2D[i].p2F.X - minX, line2D[i].p2F.Y));
        //        }
        //    if (minY < 0)
        //        for (int i = 0; i < line2D.Count(); i++)
        //        {
        //            line2D[i] = new Line2D(new PointF(line2D[i].p1F.X, line2D[i].p1F.Y - minY), new PointF(line2D[i].p2F.X, line2D[i].p2F.Y - minY));

        //        }

        //    // повторы будем убирать после размешения размеров
        //    // Llg = lg.ToList();

        //    // выполнить расчет координат точек для вставки текста
        //    for (int i = 0; i < lg.Count; i++) { lg[i] = RecalculatePointPosition(MatrixMain, lg[i], dX, dY); }

        //    // выполнить расчет координат точек для вставки текста (дуги)
        //    for (int i = 0; i < lg_arc.Count; i++) { lg_arc[i] = RecalculatePointPosition(MatrixMain, lg_arc[i], dX, dY); }

        //    // выполнить расчет координат точек для вставки текста (крюки)
        //    for (int i = 0; i < hooks.Count; i++) { hooks[i] = RecalculatePointPosition(MatrixMain, hooks[i], dX, dY, true); }

        //    SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

        //}

        /// <summary>
        /// Получить координаты точек, тип надписи и угол
        /// </summary>
        /// <param name="matrix">Матрица преобразований</param>
        /// <param name="tr">Элемент дуги</param>
        /// <param name="dX">Сдвиг по координате Х</param>
        /// <param name="dY">Сдвиг по координате Y</param>
        /// <returns>Текстовая надпись для арки</returns> 
        //TextOnArc RecalculatePointPosition(Matrix4 matrix, TextOnArc tr, float dX, float dY)
        //{
        //    Vector4 p_new = matrix.Transform(new Vector4(tr.position));                         // получить точку в локальной системе координат
        //    tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
        //    tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
        //    tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
        //    tr.incline = InclineText.Incline;                                                     // получить направление надписи
        //    // получить угол наклона надписи в градусах
        //    double dAY = (double)(tr.endF.Y - tr.startF.Y);
        //    double dAX = (double)(tr.endF.X - tr.startF.X);
        //    if (dAX == 0) tr.angle = 0;
        //    else tr.angle = (float)Math.Atan2(dAY, dAX);
        //    return tr;
        //}

        ///// <summary>
        ///// Получить координаты точек, тип надписи и угол
        ///// </summary>
        ///// <param name="matrix">Матрица преобразований</param>
        ///// <param name="tr">Элемент дуги</param>
        ///// <param name="dX">Сдвиг по координате Х</param>
        ///// <param name="dY">Сдвиг по координате Y</param>
        ///// <param name="hook">Признак расчета для крюков</param>
        ///// <returns>Текстовая надпись для прямого сегмента</returns> 
        //TextOnRebar RecalculatePointPosition(Matrix4 matrix, TextOnRebar tr, float dX, float dY, bool hook = false)
        //{
        //    Vector4 p_new = matrix.Transform(new Vector4(tr.position));                              // получить точку в локальной системе координат
        //    tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
        //    tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
        //    tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
        //    if (hook) return tr;
        //    // получить направление надписи
        //    if (tr.startF.X.Equals(tr.endF.X) && !tr.startF.Y.Equals(tr.endF.Y))
        //    {
        //        // дополнительный сдвиг для вертикальной надписи в сторону линии
        //        tr.incline = InclineText.Vertic; return tr;
        //    }
        //    if (!tr.startF.X.Equals(tr.endF.X) && tr.startF.Y.Equals(tr.endF.Y)) { tr.incline = InclineText.Horiz; return tr; }
        //    tr.incline = InclineText.Incline;
        //    // получить угол наклона надписи в градусах
        //    double dAY = (double)(tr.endF.Y - tr.startF.Y);
        //    double dAX = (double)(tr.endF.X - tr.startF.X);
        //    if (dAX == 0) tr.angle = 0;
        //    else tr.angle = (float)Math.Atan2(dAY, dAX);
        //    return tr;
        //}

        /// <summary>
        /// Инициализация данных для сегментов, радиусов загиба
        /// </summary>
        bool InitialDataForSegments()
        {            
            int num_segment = 0;
            
            for (int i = curve_start; i < curve_end; i++)
            {                
                Curve c = ilc_transform[i];
                Curve c_initial = ilc_transform_initial[i];

                // некоторые гнутые участки необходимо пропускать. Это стандартные гнутые участки, которые не имеют фактических сегментов
                if (c.GetType().Name == "Line" && lg[num_segment].arc) continue;
                if (c.GetType().Name == "Arc" && !lg[num_segment].arc) continue;

                // запишем координаты и направление сегмента
                lg[num_segment].position = (c.GetEndPoint(0) + c.GetEndPoint(1)) / 2;
                if (c.GetType().Name == "Arc")
                {
                    Arc arc = c as Arc;
                    int n= arc.Tessellate().Count / 2;
                    lg[num_segment].position = arc.Tessellate().ElementAt(n);
                }
                lg[num_segment].start = c.GetEndPoint(0);
                lg[num_segment].end = c.GetEndPoint(1);
                lg[num_segment].start_initial = c_initial.GetEndPoint(0);
                lg[num_segment].end_initial = c_initial.GetEndPoint(1);
                num_segment++;
            }
            // проверка наличия позиций у сегментов. Если позиции нет - картинки не будет. Какое-то несоответствие. Возможно стержень 3d или самопальное семейство
            foreach (TextOnRebar tor in lg)
            {
                if (tor.position == null) return false;
            }            

            return true;
        }


        /// <summary>
        /// Получить кривые стержня 
        /// </summary>
        public void GetRebarsCurves()
        {
            // готовим новые линии для вычерчивания стержня 
            // координаты линий назначаются по основному стержню (необязательно, что эти линии лежат в плоскости вида.
            // даже, если стержень виден на виде

            ilc.Clear();
            ilc_lines.Clear();
            // здесь выполняем 
            if (rebarOne != null)
            {               
                doc = rebarOne.Document;
                // получить данные по форме стержня
                ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
                // rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
                rs = rebarOne.Document.GetElement(rebarOne.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()) as RebarShape;
                foreach (Curve c in ilc)
                {
                    if (c.GetType().Name == "Line") ilc_lines.Add(c);
                }
                // без 1 крюка
                if (hook_start > 0) ilc_lines.RemoveAt(0);
                

            }
            if (rebarIn != null)
            {
                doc = rebarIn.Document;
                // получить данные по форме стержня                
                ilc = rebarIn.GetCenterlineCurves(false, false, false);
                rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
                foreach (Curve c in ilc)
                {
                    if (c.GetType().Name == "Line") ilc_lines.Add(c);
                }
                // без 1 крюка
                if (hook_start > 0) ilc_lines.RemoveAt(0);

            }

            rsd = rs.GetRebarShapeDefinition();
            rarc = rsd as RebarShapeDefinitionByArc;     
            if(rarc!=null)
            {
                if (rarc.Type == RebarShapeDefinitionByArcType.Spiral)
                {
                    IsHermiteSpline = true;
                    DirMainSegment = active_view.UpDirection;
                }
            }
            rsds = rsd as RebarShapeDefinitionBySegments;            
            GetDirMainSegment();          
        }



        /// <summary>
        /// Формирование данных по участкам армирования - прямые участки
        /// </summary>
        void DataBySegments()
        {
            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
            ParameterSet pset = rebar.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 
            lg.Clear();
            // Цикл по сегментам в данной форме rsds.NumberOfSegments
            for (int i = 0; i < rsds.NumberOfSegments; i++)
            {
                TextOnRebar tor = new TextOnRebar();                                      // создаем будущую надпись над сегментом
                tor.rebar = rebar;                                                        // запишем текущий стержень
                RebarShapeSegment segment = rsds.GetSegment(i);                           // определяем сегмент

                IList<RebarShapeConstraint> ILrsc = segment.GetConstraints();             // параметры сегмента                

                foreach (RebarShapeConstraint rsc in ILrsc)                               // разбираем каждый сегмент в отдельности
                {
                    // получим длину сегмента
                    RebarShapeConstraintSegmentLength l = rsc as RebarShapeConstraintSegmentLength;

                    if (l != null)
                    {
                        ElementId pid = l.GetParamId();
                        Element elem = doc.GetElement(pid);
                        foreach (Parameter pr in pset)
                        {
                            if (pr.Definition.Name == elem.Name)
                            {
                                tor.guid = pr.GUID;
                                if (pr.HasValue)
                                {
                                    // с учетом локальных особенностей
                                    tor.value_initial = rebar.get_Parameter(pr.Definition).AsDouble();
                                    tor.value = tor.value_initial; //  + GetAddValue(template, tor.guid, rebar as Rebar);
                                    if (tor.value <= 0) tor.value = tor.value_initial;
                                    if (tor.value > 0) break;
                                }
                                else // для участков стержней переменной длины
                                {
                                    double l_min = 0;
                                    tor.value_initial = tor.value = GetMaxMinValue(rebar as Rebar, pr, out l_min);
                                    tor.value_min = l_min;
                                }
                            }
                        }

                        // запишем для контроля имя параметра
                        tor.name = elem.Name;                                                                          // добавим метку
                        // определяем, что данный сегмент является дугой
                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeDefaultBend");
                            tor.arc = true;
                        }
                        catch { }
                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendRadius");
                            tor.arc = true;
                        }
                        catch { }

                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendArcLength");
                            tor.arc = true;
                        }
                        catch { }
                        continue;
                    }
                    
                }
                
                lg.Add(tor);   // внесем сегмент в общий список
            }

        }


        /// <summary>
        /// Получить максимальную и минимальную длину сегмента стержня для указанного параметра
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="param">Параметр</param>
        /// <param name="min">Минимальная длина сегмента</param>
        /// <returns>Максимальная длина сегмента</returns>
        double GetMaxMinValue(Rebar rebar, Parameter param, out double min)
        {
            min = 0;  // минимальная длина сегмента
            if (param.HasValue) return rebar.get_Parameter(param.Definition).AsDouble();
            int segments = rebar.NumberOfBarPositions;
            if (segments <= 0) return 0;
            DoubleParameterValue minV = rebar.GetParameterValueAtIndex(param.Id, segments - 1) as DoubleParameterValue;
            DoubleParameterValue maxV = rebar.GetParameterValueAtIndex(param.Id, 0) as DoubleParameterValue;
            if (maxV.Value > minV.Value) { min = minV.Value; return maxV.Value; }
            else { min = maxV.Value; return minV.Value; }

        }        

        /// <summary>
        /// Формирование данных по участкам армирования - арка
        /// </summary>
        void DataByArcs()
        {
            lg.Clear();
            // тексты для отображения
            TextOnRebar X_max = new TextOnRebar();
            //TextOnRebar X_max_d = new TextOnRebar();
            TextOnRebar Y_max = new TextOnRebar();
            //TextOnRebar Y_max_d = new TextOnRebar();

            XYZ extr1 = outline.MaximumPoint;
            XYZ extr2 = outline.MinimumPoint;
            XYZ middl = (extr1 + extr2) / 2;
            double dist = extr1.DistanceTo(extr2);
            XYZ vec = (extr1 - extr2).Normalize();
            double angleX = vec.AngleTo(active_view.RightDirection);
            double angleY = vec.AngleTo(active_view.UpDirection);

            // для арочных стержней показываем горизонтальную и вертикальную проекции
            // диагонали внешнего контура. Формируем их как два значения
            X_max.rebar = rebar;
            X_max.value = Math.Abs(dist * Math.Cos(angleX)); //  + Diametr;
            if (detailCurves.Count == 0) X_max.value = X_max.value + Diametr;
            Line rawY = Line.CreateUnbound(middl, active_view.UpDirection);
            Line rawX = Line.CreateUnbound(extr2, active_view.RightDirection);
            IntersectionResultArray ira = null;
            rawY.Intersect(rawX, out ira);
            if (ira != null)
            {

                XYZ cross = ira.get_Item(0).XYZPoint;
                if (cross==null) { lg.Clear(); return; }  // что-то не получилось
                X_max.position = cross;
                X_max.angle = 0;
                X_max.start = new XYZ(0, 0, 0);
                X_max.end = active_view.RightDirection;
                X_max.start_initial = new XYZ(0, 0, 0);
                X_max.end_initial = active_view.RightDirection;
                lg.Add(X_max);
            }

            Y_max.rebar = rebar;
            Y_max.value = Math.Abs(dist * Math.Cos(angleY)); //  + Diametr;
            if (detailCurves.Count == 0) Y_max.value = Y_max.value + Diametr;
            rawY = Line.CreateUnbound(middl, active_view.RightDirection);
            rawX = Line.CreateUnbound(extr2, active_view.UpDirection);
            ira = null;
            rawY.Intersect(rawX, out ira);
            if (ira != null)
            {
                XYZ cross = ira.get_Item(0).XYZPoint;
                if (cross == null) { lg.Clear(); return; }  // что-то не получилось
                Y_max.position = cross;
                Y_max.angle = Math.PI/2;
                Y_max.start = new XYZ(0, 0, 0);
                Y_max.end = active_view.UpDirection;
                Y_max.start_initial = new XYZ(0, 0, 0);
                Y_max.end_initial = active_view.UpDirection;
                lg.Add(Y_max);
            }


            //Line line = Line.CreateBound(outline.MaximumPoint,outline.MinimumPoint);
            //// Покажем контуры чертежа
            //DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, line);
            //newcurve.LineStyle = gs_test;
            //Eids.Add(newcurve.Id);                     // все кривые показываем в группе чертежа



            //// получить координаты крайних точек
            //double minX, minY, maxX, maxY;

            //SketchTools.GetExtremePoints(ilc_transform, out minX, out minY, out maxX, out maxY);
            //X_max.rebar = rebar;
            //X_max.value = Math.Abs(maxX - minX) + Diametr;
            //X_max.position = new XYZ((maxX + minX) / 2, minY, ilc_transform.First().GetEndPoint(0).Z);
            //X_max.angle = 0;
            //X_max.start = new XYZ(0, 0, 0);
            //X_max.end = active_view.RightDirection;
            //lg.Add(X_max);

            //Y_max.rebar = rebar;
            //Y_max.value = Math.Abs(maxY - minY) + Diametr;
            //Y_max.position = new XYZ(minX, (maxY + minY) / 2, ilc_transform.First().GetEndPoint(0).Z);
            //Y_max.angle = Math.PI / 2;
            //Y_max.start = new XYZ(0, 0, 0);
            //Y_max.end = active_view.UpDirection;
            //lg.Add(Y_max);

            //Line line = Line.CreateBound(new XYZ(minX, minY, ilc_transform.First().GetEndPoint(0).Z), new XYZ(maxX, maxY, ilc_transform.First().GetEndPoint(0).Z));
            //// Покажем контуры чертежа
            //DetailCurve newcurve = doc.Create.NewDetailCurve(active_view, line);
            //newcurve.LineStyle = gs_test;
            //Eids.Add(newcurve.Id);                     // все кривые показываем в группе чертежа



        }



        ///// <summary>
        ///// Получить дополнительное локальное значение
        ///// </summary>
        ///// <param name="template">Шаблон проекта</param>
        ///// <param name="parameter">Параметр</param>        
        ///// <returns>Дополнительное значение</returns>         

        //static double GetAddValue(Template template, Guid parameter, Element rebar)
        //{
        //    if (template == Template.Other) return 0;
        //    string form = "";
        //    double diam = 0;
        //    Rebar rebarOne = rebar as Rebar;
        //    RebarInSystem rebarIn = rebar as RebarInSystem;

        //    // получить менеджер текущего стержня
        //    if (rebarOne != null)
        //    {
        //        form = rebarOne.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
        //        diam = rebarOne.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
        //    }
        //    if (rebarIn != null)
        //    {
        //        form = rebarIn.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
        //        diam = rebarIn.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
        //    }

        //    LegGuid guid = new LegGuid();

        //    switch (form)
        //    {

        //        case "10":
        //            // минус два диаметра - для получения внутреннего размера хомута
        //            return -2 * diam;
        //        case "26":
        //            // минус два диаметра - для получения внутреннего размера хомута
        //            if (parameter.Equals(guid.A)) return -2 * diam;
        //            if (parameter.Equals(guid.D)) return -2 * diam;
        //            break;
        //        default:
        //            return 0;
        //    }
        //    return 0;
        //}

        ///// <summary>
        ///// Изменить параметры стержня
        ///// </summary>
        //void ChangeParametersRebar()
        //{
        //    // попытаемся обойтись без транзакций
        //    //transaction.Commit();     // запишем исходное состояние
        //    //FailureHandlingOptions options = transaction.GetFailureHandlingOptions();
        //    //FailurePreproccessor preproccessor = new FailurePreproccessor();
        //    //options.SetFailuresPreprocessor(preproccessor);
        //    //transaction.SetFailureHandlingOptions(options);
        //    //transaction.Start();

        //    // failureOptions.SetFailuresPreprocessor(new HideNewTypeAssembly());

        //    SubTransaction sudst = new SubTransaction(doc);
        //    sudst.Start();

        //    if (hooks.Count > 0)   // при наличии крюков - масштабируем по крюкам
        //    {

        //        // получить максимальную длину крюка
        //        double max_hook = hooks.Min(x => x.value);
        //        double max_segment = lg.Max(x => x.value);
        //        double coeff = (max * max_hook) / max_segment;
        //        if (max_segment == 0) coeff = 1;
        //        // coeff = coeff / coef_hook;
        //        // изменим параметры
        //        int i = 0;
        //        foreach (TextOnRebar tor in lg)
        //        {
        //            // double coeff = tor.value / max_segment * coef_hook;

        //            double value = tor.value * coeff;
        //            //if (value < min * max_hook) value = min * max_hook * coef_hook;
        //            //if (value > max * max_hook) value = max * max_hook * coef_hook;
        //            if (tor.dialog) value = value * coef[i];                              // изменяем только для параметров включенных в диалог
        //            else { i++; continue; }
        //            SketchTools.SetParameter(rebar, tor.guid, value);
        //            // изменить длины проекций
        //            if (tor.valueH > 0)
        //            {
        //                value = tor.valueH * coeff;
        //                //if (value < min * max_hook) value = min * max_hook * coef_hook;
        //                //if (value > max * max_hook) value = max * max_hook * coef_hook;
        //                if (tor.dialog) value = value * coef[i];
        //                SketchTools.SetParameter(rebar, tor.guidH, value);
        //            }
        //            // изменить длины проекций
        //            if (tor.valueV > 0)
        //            {
        //                value = tor.valueV * coeff;
        //                //if (value < min * max_hook) value = min * max_hook * coef_hook;
        //                //if (value > max * max_hook) value = max * max_hook * coef_hook;
        //                if (tor.dialog) value = value * coef[i];
        //                SketchTools.SetParameter(rebar, tor.guidV, value);
        //            }
        //            i++;
        //        }

        //        // doc.Regenerate();
        //    }
        //    else
        //    {
        //        double max_segment = lg.Max(x => x.value);
        //        // изменим параметры
        //        int i = 0;
        //        foreach (TextOnRebar tor in lg)
        //        {

        //            double new_value = tor.value;
        //            if (new_value < max_segment / 4) new_value = max_segment / 4;
        //            if (tor.dialog) new_value = new_value * coef[i];   // изменяем только для параметров включенных в диалог
        //            else { i++; continue; }
        //            SketchTools.SetParameter(rebar, tor.guid, new_value);
        //            i++;
        //        }

        //        // doc.Regenerate();
        //    }


        //    doc.Regenerate();
        //    PreparedDataSegements();       // получить новые кривые 
        //    GetInfoAboutHooks();       // обновить информацию по крюкам
        //    sudst.RollBack();

        //    // восстановить старые значения сегментов
        //    foreach (TextOnRebar tor in lg)
        //    {
        //        SketchTools.SetParameter(rebar, tor.guid, tor.value_initial);

        //    }
        //    doc.Regenerate();


        //    // transaction.RollBack();

        //    //if (!preproccessor.status) status = false;
        //    //else
        //    //{
        //    //GetInfoAboutHooks();       // обновить информацию по крюкам 
        //    //status = true;
        //    //}
        //    //transaction.Start();


        //}

        void GetInfoAboutHooks()
        {
            // обновить информацию по крюкам - полная длина крюка
            hooks.Clear();            
            // получить информацию по крюкам (начало)
            if (hook_start > 0)
            {
                if(rebarOne!=null) hooks.Add(GetHookStart(ilc_transform, rebarOne));
                if (rebarIn != null) hooks.Add(GetHookStart(ilc_transform, rebarIn));// добавим информацию по крюку
                hooks.Last().isHookStart = true;
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(0)) as DetailCurve).GeometryCurve);
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(1)) as DetailCurve).GeometryCurve);
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(2)) as DetailCurve).GeometryCurve);
            }
            else
            {
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(0)) as DetailCurve).GeometryCurve);
                if(Eids_lines.Count>1) cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(1)) as DetailCurve).GeometryCurve);
            }

            int i = Eids_lines.Count - 1;
            if (hook_end > 0)
            {

                if (rebarOne != null)  hooks.Add(GetHookEnd(ilc_transform, rebarOne));                                                     // добавим информацию по крюку
                if (rebarIn != null) hooks.Add(GetHookEnd(ilc_transform, rebarIn));
                hooks.Last().isHookEnd = true;
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(i)) as DetailCurve).GeometryCurve);
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(i-1)) as DetailCurve).GeometryCurve);
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(i - 2)) as DetailCurve).GeometryCurve);
            }
            else
            {
                cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(i)) as DetailCurve).GeometryCurve);
                if (Eids_lines.Count > 1) cross_lines_hooks.Add((doc.GetElement(Eids_lines.ElementAt(i-1)) as DetailCurve).GeometryCurve);
            }

            // получить плоскость на которой находятся линии
            work_plane_drawing = new Plane3D(cross_lines_hooks[0].GetEndPoint(0), 
                                                     cross_lines_hooks[0].GetEndPoint(1),
                                                     cross_lines_hooks[1].GetEndPoint(1));


        }

        /// <summary>
        /// Получить параметры для начального крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        static TextOnRebar GetHookStart(IList<Curve> curves, Rebar rebar)
        {
            RebarBendData rbd = rebar.GetBendData();           
            Curve c_straight = curves[0];
            Curve c_arc = curves[1];
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar as Element;
            tor.position = (c_straight.GetEndPoint(1) + c_straight.GetEndPoint(0)) / 2;
            // tor.value = Math.Round(rbd.HookLength0 + rbd.BendRadius + rbd.BarModelDiameter,3);
            tor.value = Math.Round(rbd.HookLength0 + rbd.HookBendRadius + rbd.BarModelDiameter, 3);
            if (rbd.HookAngle0 < 90) tor.value = Math.Round(rbd.HookLength0 + c_arc.ApproximateLength,3);
            // вектор сегмента должен быть направлен к концу стержня
            // чтобы можно было разместить текст на конце
            tor.start = c_straight.GetEndPoint(1);
            tor.end = c_straight.GetEndPoint(0);
            tor.end_initial = c_straight.GetEndPoint(1);
            tor.start_initial = c_straight.GetEndPoint(0);
            return tor;
        }
        /// <summary>
        /// Получить параметры для начального крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        static TextOnRebar GetHookStart(IList<Curve> curves, RebarInSystem rebar)
        {
            RebarBendData rbd = rebar.GetBendData();
            Curve c_straight = curves[0];
            Curve c_arc = curves[1];
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar as Element;
            tor.position = (c_straight.GetEndPoint(1) + c_straight.GetEndPoint(0)) / 2;
            // tor.value = Math.Round(rbd.HookLength0 + rbd.BendRadius + rbd.BarModelDiameter,3);
            tor.value = Math.Round(rbd.HookLength0 + rbd.HookBendRadius + rbd.BarModelDiameter, 3);
            if (rbd.HookAngle0 < 90) tor.value = Math.Round(rbd.HookLength0 + c_arc.ApproximateLength,3);
            // вектор сегмента должен быть направлен к концу стержня
            // чтобы можно было разместить текст на конце
            tor.start = c_straight.GetEndPoint(1);
            tor.end = c_straight.GetEndPoint(0);
            tor.end_initial = c_straight.GetEndPoint(1);
            tor.start_initial = c_straight.GetEndPoint(0);
            return tor;
        }
        /// <summary>
        /// Получить параметры для конечного крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        static TextOnRebar GetHookEnd(IList<Curve> curves, Rebar rebar)
        {
            RebarBendData rbd = rebar.GetBendData();
            Curve c_straight = curves.Last();
            Curve c_arc = curves[curves.Count - 2];
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar as Element;
            tor.position = (c_straight.GetEndPoint(1) + c_straight.GetEndPoint(0)) / 2;
            // tor.value = Math.Round(rbd.HookLength1 + rbd.BendRadius + rbd.BarModelDiameter,3);
            tor.value = Math.Round(rbd.HookLength1 + rbd.HookBendRadius + rbd.BarModelDiameter, 3);
            if (rbd.HookAngle1 < 90) tor.value = Math.Round(rbd.HookLength0 + c_arc.ApproximateLength,3);
            // вектор сегмента должен быть направлен к концу стержня
            // чтобы можно было разместить текст на конце
            tor.start = c_straight.GetEndPoint(0);
            tor.end = c_straight.GetEndPoint(1);
            tor.end_initial = c_straight.GetEndPoint(0);
            tor.start_initial = c_straight.GetEndPoint(1);
            return tor;
        }
        /// <summary>
        /// Получить параметры для конечного крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        static TextOnRebar GetHookEnd(IList<Curve> curves, RebarInSystem rebar)
        {
            RebarBendData rbd = rebar.GetBendData();
            Curve c_straight = curves.Last();
            Curve c_arc = curves[curves.Count - 2];
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar as Element;
            tor.position = (c_straight.GetEndPoint(1) + c_straight.GetEndPoint(0))/2;
            // tor.value = Math.Round(rbd.HookLength1 + rbd.BendRadius + rbd.BarModelDiameter,3);
            tor.value = Math.Round(rbd.HookLength1 + rbd.HookBendRadius + rbd.BarModelDiameter, 3);
            if (rbd.HookAngle1 < 90) tor.value = Math.Round(rbd.HookLength0 + c_arc.ApproximateLength,3);
            // вектор сегмента должен быть направлен к концу стержня
            // чтобы можно было разместить текст на конце
            tor.start = c_straight.GetEndPoint(1);
            tor.end = c_straight.GetEndPoint(0);
            tor.end_initial = c_straight.GetEndPoint(0);
            tor.start_initial = c_straight.GetEndPoint(1);
            tor.arc = true;
            return tor;
        }


    }

}
