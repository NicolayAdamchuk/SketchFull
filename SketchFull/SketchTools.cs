using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Drawing;


namespace SketchFull
{
    ///// <summary>
    ///// Implements the interface IFailuresPreprocessor
    ///// </summary>
    //public class FailurePreproccessor : IFailuresPreprocessor
    //{
    //    public bool status = true;
    //    /// <summary>
    //    /// This method is called when there have been failures found at the end of a transaction and Revit is about to start processing them. 
    //    /// </summary>
    //    /// <param name="failuresAccessor">The Interface class that provides access to the failure information. </param>
    //    /// <returns></returns>
    //    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
    //    {
    //        IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
    //        if (fmas.Count == 0)
    //        {
    //            return FailureProcessingResult.Continue;
    //        }
    //        else
    //        {
    //            status = false;
    //        }

    //        //failuresAccessor.DeleteAllWarnings();


    //        //foreach (FailureMessageAccessor fma in fmas)
    //        //{
    //        //    FailureDefinitionId id = fma.GetFailureDefinitionId();

    //        //    string st = fma.GetDescriptionText();

    //        //    //if (id == Command.m_idWarning)
    //        //    //{
    //        //    //    failuresAccessor.DeleteWarning(fma);
    //        //    //}
    //        //}

    //        //    failuresAccessor.RollBackPendingTransaction();
    //            return FailureProcessingResult.ProceedWithRollBack; // .ProceedWithCommit;            
    //    }
    //}
    ///// <summary>
    ///// A failure preprocessor to hide the warning about duplicate types being pasted.
    ///// </summary>
    //class HideNewTypeAssembly : IFailuresPreprocessor
    //{
    //    #region IFailuresPreprocessor Members

    //    /// <summary>
    //    /// Implementation of the IFailuresPreprocessor.
    //    /// </summary>
    //    /// <param name="failuresAccessor"></param>
    //    /// <returns></returns>
    //    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
    //    {
    //        failuresAccessor.DeleteAllWarnings();
    //        //foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
    //        //{
    //        //    // Delete any "Can't paste duplicate types.  Only non duplicate types will be pasted." warnings
    //        //    //if (failure.GetFailureDefinitionId() ==  BuiltInFailures.CopyPasteFailures.CannotCopyDuplicates)
    //        //    if (failure.GetFailureDefinitionId() == BuiltInFailures.AssemblyFailures.AssemblyNewTypeWarn)
    //        //    {
    //        //    failuresAccessor.DeleteWarning(failure);
    //        //    }


    //        //}

    //        // Handle any other errors interactively
    //        return FailureProcessingResult.Continue;
    //    }

    //    #endregion
    //}
    /// <summary>
    /// Режим работы генерации эскизов
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Все стержни
        /// </summary>
        All,
        /// <summary>
        /// Отдельный стержень
        /// </summary>
        Single,
        /// <summary>
        /// По главному сегменту
        /// </summary>
        MainSegment
    }
    /// <summary>
    /// 3 точки на плоскости
    /// </summary>
    public class Plane3D
    {
        public XYZ p1;
        public XYZ p2;
        public XYZ p3;
        public Plane3D(XYZ p1, XYZ p2, XYZ p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }
    /// <summary>
    /// Группы на виде
    /// </summary>
    class GroupOnView
    {
        /// <summary>
        /// Точка вставки группы
        /// </summary>
        public XYZ insert = XYZ.Zero;
        /// <summary>
        /// Признак финального размещения группы
        /// </summary>
        public bool finish_pos = false;
        /// <summary>
        /// Финальное перемещение группы
        /// </summary>
        public double finish_dist = 0;
        /// <summary>
        /// Вектор перемещения группы
        /// </summary>
        public XYZ move = XYZ.Zero;
        /// <summary>
        /// Начало линии эскиза
        /// </summary>
        public XYZ p1 = XYZ.Zero;
        /// <summary>
        /// Конец линии эскиза
        /// </summary>
        public XYZ p2 = XYZ.Zero;
        /// <summary>
        /// Высота группы на текущем виде
        /// </summary>
        public double groupH = 0;
        /// <summary>
        /// Группа
        /// </summary>
        public Group group;
        /// <summary>
        /// Направление основного сегмента в модели
        /// </summary>
        public XYZ DirMainSegment;
        ///// <summary>
        ///// Основной элемент в группе
        ///// </summary>
        //public DetailCurve MainDetailCurve;

        /// <summary>
        /// Основная линия в группе
        /// </summary>
        public Line MainLine;
        /// <summary>
        /// Направление основного сегмента в группе
        /// </summary>
        public XYZ DirMainDetailCurve
        {
            get
            {
                if (MainLine != null)
                {
                    return (MainLine.GetEndPoint(1) - MainLine.GetEndPoint(0)).Normalize();  // направление основного сегмента на чертеже
                }
                return null;
            }
        }
        /// <summary>
        /// Середина основного основного сегмента в группе
        /// </summary>
        public XYZ MiddleMainDetailCurve
        {
            get
            {
                if (MainLine != null)
                {
                    return (MainLine.GetEndPoint(1) + MainLine.GetEndPoint(0)) / 2;  // направление основного сегмента на чертеже
                }
                return null;
            }
        }


        /// <summary>
        /// Основная линия в группе
        /// </summary>
        public Line MainLineScale;
        /// <summary>
        /// Направление основного сегмента в группе
        /// </summary>
        public XYZ DirMainDetailCurveScale
        {
            get
            {
                if (MainLineScale != null)
                {
                    return (MainLineScale.GetEndPoint(1) - MainLineScale.GetEndPoint(0)).Normalize();  // направление основного сегмента на чертеже
                }
                return null;
            }
        }
        /// <summary>
        /// Середина основного основного сегмента в группе
        /// </summary>
        public XYZ MiddleMainDetailCurveScale
        {
            get
            {
                if (MainLineScale != null)
                {
                    return (MainLineScale.GetEndPoint(1) + MainLineScale.GetEndPoint(0)) / 2;  // направление основного сегмента на чертеже
                }
                return null;
            }
        }


        ///// <summary>
        ///// Направление основного сегмента в группе
        ///// </summary>
        //public XYZ DirMainDetailCurve
        //{
        //    get
        //    {
        //        if (MainDetailCurve != null)
        //        {
        //            Curve main_on_view = MainDetailCurve.GeometryCurve;
        //            return (main_on_view.GetEndPoint(1) - main_on_view.GetEndPoint(0)).Normalize();  // направление основного сегмента на чертеже
        //        }
        //        return null;
        //    }
        //}
        ///// <summary>
        ///// Середина основного основного сегмента в группе
        ///// </summary>
        //public XYZ MiddleMainDetailCurve
        //{
        //    get
        //    {
        //        if (MainDetailCurve != null)
        //        {
        //            Curve main_on_view = MainDetailCurve.GeometryCurve;
        //            return (main_on_view.GetEndPoint(1) + main_on_view.GetEndPoint(0)) / 2;  // направление основного сегмента на чертеже
        //        }
        //        return null;
        //    }
        //}
        /// <summary>
        /// Признак расположения плоскости стержня на виде
        /// </summary>
        public bool isRebarOnView;
        /// <summary>
        /// Проекция основного сегмента на виде (точка)
        /// </summary>
        public XYZ project_point;
        /// <summary>
        /// Элемент - хозяин эскиза
        /// </summary>
        public Element element;
        public GroupOnView()
        {

        }
        //public GroupOnView(Element element, Group group, bool isRebarOnView, XYZ project_point)
        //{
        //    this.element = element;
        //    this.group = group;
        //    this.isRebarOnView = isRebarOnView;
        //    this.project_point = project_point;
        //}
    }

    /// <summary>
    /// Направление армирования по площади
    /// </summary>
    public enum AreaDirect
    {
        Main,
        Second
    }
    /// <summary>
    /// Слой армирования по площади
    /// </summary>
    public enum AreaLayer
    {
        Up,
        Down
    }

    /// <summary>
    /// Направление генерации эскизов
    /// </summary>
    public enum SketchDirect
    {
        Down,
        Up,
        Left,
        Right
    }
    ///// <summary>
    ///// Признак шаблона  
    ///// </summary>
    //public enum Template
    //{
    //    /// <summary>
    //    /// Русский
    //    /// </summary>
    //    Rus,
    //    /// <summary>
    //    /// Прочий
    //    /// </summary>
    //    Other
    //}

    /// <summary>
    /// Признак наклона надписи  
    /// </summary>
    public enum InclineText
    {
        /// <summary>
        /// Горизонтально
        /// </summary>
        Horiz,
        /// <summary>
        /// Вертикально
        /// </summary>
        Vertic,
        /// <summary>
        /// Под уголом
        /// </summary>
        Incline,
        /// <summary>
        /// Радиус
        /// </summary>
        Radius

    }
    /// <summary>
    /// Плоские линии для чертежей
    /// </summary>
    public class Line2D
    {
        /// <summary>
        /// Точки линии
        /// </summary>
        public PointF p1F, p2F;
        /// <summary>
        /// Точки линии
        /// </summary>
        public XYZ p1, p2;
        /// <summary>
        /// Линия 2D - Z=0;
        /// </summary>        
        public Line line
        {
            get
            {
                if (p1.DistanceTo(p2) < 0.001) return null;   // линия слишком короткая
                return Line.CreateBound(p1, p2);
            }
        }

        /// <summary>
        /// Получить плоскую линию для чертежа
        /// </summary>
        /// <param name="p1">Начальная точка</param>
        /// <param name="p2">Конечная точка</param>         
        /// <returns>Плоская линия Z=0</returns> 
        public Line2D(PointF p1, PointF p2)
        {
            this.p1F = p1;
            this.p2F = p2;
            this.p1 = new XYZ(p1.X, p1.Y, 0);
            this.p2 = new XYZ(p2.X, p2.Y, 0);
        }

    }
    /// <summary>
    /// Данные диалога
    /// </summary>
    public class DataForm
    {
        /// <summary>
        /// Показать длину крюков
        /// </summary>
        public bool HooksLength = false;
        /// <summary>
        /// Использовать все стержни модели
        /// </summary>
        public bool AllRebars = true;
        /// <summary>
        /// Показать радиус загиба
        /// </summary>
        public bool BendingRadius = false;
        /// <summary>
        /// Обновить ручные исправления
        /// </summary>
        public bool UpdateSingleRebar = false;

    }

    /// <summary>
    /// Фильтр выбора для элементов армирования
    /// </summary>
    public class TargetElementSelectionFilter : ISelectionFilter
    {

        public bool AllowElement(Element element)
        {

            if (element.GetType().Name.Equals("Rebar") ||
                element.GetType().Name.Equals("RebarInSystem") ||
                element.GetType().Name.Equals("AreaReinforcement") ||
                element.GetType().Name.Equals("PathReinforcement"))
            {
                //RebarShape rs = null;
                //Rebar rebarOne = element as Rebar;
                //RebarInSystem rebarIn = element as RebarInSystem;
                //// здесь выполняем разделение по типам возможного армирования: отдельные стержни или стержни в системе
                //// получить данные по форме стержня
                //if (rebarOne != null) rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
                //if (rebarIn != null) rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;

                //RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
                //RebarShapeDefinitionByArc rarc = rsd as RebarShapeDefinitionByArc;
                //RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
                //if (rsds == null && rarc == null) return false;   // формы не определяются
                //// if (rarc != null) return false;                   // арочную форму пропускаем      
                return true;
            }
            return false;

        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
    }

    /// <summary>
    /// Фильтр выбора для элементов армирования
    /// </summary>
    public class TargetConcreteElement : ISelectionFilter
    {

        public bool AllowElement(Element element)
        {

            Category c = element.Category;

            if (((BuiltInCategory)c.Id.IntegerValue).ToString() == BuiltInCategory.OST_Floors.ToString() ||
                ((BuiltInCategory)c.Id.IntegerValue).ToString() == BuiltInCategory.OST_Walls.ToString() ||
                ((BuiltInCategory)c.Id.IntegerValue).ToString() == BuiltInCategory.OST_StructuralFraming.ToString() ||
                ((BuiltInCategory)c.Id.IntegerValue).ToString() == BuiltInCategory.OST_StructuralColumns.ToString() ||
                ((BuiltInCategory)c.Id.IntegerValue).ToString() == BuiltInCategory.OST_StructuralFoundation.ToString())
            {
                ElementCategoryFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rebar);               
                //ElementFilter elementFilter = null;
                //elementFilter.PassesFilter(wallFilter);
                IList<ElementId> elementIds = element.GetDependentElements(wallFilter);
                if (elementIds.Count > 0) return true;
                else      return false;
                //ElementId eid = element.GetAnalyticalModelId();
                //if (eid == null) return false;
                //if (eid.IntegerValue < 0) return false;
                // return true;
            }
            else return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
    }

    /// <summary>
    /// Маркировка арматуры
    /// </summary>
    class MarkR : IEquatable<MarkR>
    {
        public double Length;
        public string bar, forma, segments;


        public MarkR(string segments, string bar, string forma, double Length)
        {
            this.bar = bar;
            this.Length = Length;
            this.forma = forma;
            this.segments = segments;
        }

        public bool Equals(MarkR other)
        {

            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            // return Length.Equals(other.Length) && bar.Equals(other.bar) && forma.Equals(other.forma) && segments.Equals(other.segments);
            return bar.Equals(other.bar) && forma.Equals(other.forma) && segments.Equals(other.segments);
        }


        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public override int GetHashCode()
        {

            ////Get hash code for the Name field if it is not null.
            //int hashProductGost = Length == null ? 0 : Length.GetHashCode();

            int hashProductClass = forma == null ? 0 : forma.GetHashCode();

            int hashProductBar = bar == null ? 0 : bar.GetHashCode();

            int hashProductSegment = segments == null ? 0 : segments.GetHashCode();

            //Calculate the hash code for the product.
            // return hashProductGost ^ hashProductClass ^ hashProductBar ^ hashProductSegment;
            return hashProductClass ^ hashProductBar ^ hashProductSegment;
        }

    }



    /// <summary>
    /// Guid участков арматурных стержней
    /// </summary>
    public class LegGuid
    {
        public Guid A, B, C, D, E, F, G, H, J, h1, h2;


        public LegGuid()
        {

            A = new Guid("b5ef18b4-453e-49bd-b26c-dfb3bd3ca79c");
            h1 = new Guid("a4d54aaa-6132-4af4-84ce-8638096c9941");  // Крюк прямого стержня                 
            h2 = new Guid("bb67f21c-3436-4e0e-ae86-12a7b20567c9");  // Крюк прямого стержня
            B = new Guid("bef64550-0992-4b59-a616-1acaa2e24065");
            C = new Guid("4d1d1719-6bd9-4357-9378-a1d77871e0fd");
            D = new Guid("93ddaf87-08af-4bb9-b48f-87994feec729");
            F = new Guid("99509457-fdd5-40cf-a4cd-522b20acdd64");
            E = new Guid("ba55593e-d70c-410c-ba60-6e935aa1c169");
            G = new Guid("64aa0034-0c4d-400a-b048-d40e47637914");
            H = new Guid("098420cf-d8fe-4c71-939b-fc441b9ffcae");
            J = new Guid("750b510b-4034-403d-afa7-436272cffa36");
        }
    }
    /// <summary>
    /// Получение парметров и констант
    /// </summary>
    class SketchTools
    {
        /// <summary>
        /// Получить контур на виде для зоны черчения
        /// </summary>
        public static Outline GetOutlineForCrop(Autodesk.Revit.DB.View view, Plane3D plane3D)
        {
            Outline outline = null;
            if (!view.CropBoxActive) return null;
            BoundingBoxXYZ bb = view.CropBox;
            XYZ bbMax = ProjectPointOnWorkPlane(plane3D, bb.Transform.OfPoint(bb.Max));
            XYZ bbMin = ProjectPointOnWorkPlane(plane3D, bb.Transform.OfPoint(bb.Min));
            // лучи ограничивающие контур эскиза
            Line line_up = Line.CreateUnbound(bbMax, view.RightDirection);
            Line line_down = Line.CreateUnbound(bbMin, view.RightDirection);

            Line line_left = Line.CreateUnbound(bbMax, view.UpDirection);
            Line line_right = Line.CreateUnbound(bbMin, view.UpDirection);

            XYZ p1 = null;
            XYZ p2 = null;
            IntersectionResultArray ira = null;
            line_up.Intersect(line_right, out ira);
            if (ira != null)
            {
                p1 = ira.get_Item(0).XYZPoint;
            }
            line_left.Intersect(line_down, out ira);
            if (ira != null)
            {
                p2 = ira.get_Item(0).XYZPoint;
            }
            if (p1 != null && p2 != null)
            {
                outline = new Outline(bbMax, bbMin);
                outline.AddPoint(p1); outline.AddPoint(p2);
            }

            return outline;
        }
        /// <summary>
        /// Получить вектор нормали (аналитический)
        /// </summary>
        static XYZ AnalyticalNormal(Element element)
        {
            switch (element.Category.Id.IntegerValue)
            {
                case (int)BuiltInCategory.OST_Floors:
                    return XYZ.BasisZ;
                    //AnalyticalModel am = element.GetAnalyticalModel();
                    //return am.GetLocalCoordinateSystem().BasisZ;
                case (int)BuiltInCategory.OST_Walls:
                    return (element as Wall).Orientation;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Получить слой армирования для RebarInSystem
        /// </summary>
        public static AreaLayer GetLayerForRebarInSystem(RebarInSystem rebarInSystem, List<Plane3D> plane3D)
        {
            IList<Curve> curves = rebarInSystem.GetCenterlineCurves(false, false, false);
            XYZ p = XYZ.Zero;
            if (curves.Count > 1) // фактически есть крюк - берем уровень прямого участка
                p = curves[2].GetEndPoint(0); // точка стержня
            else
                p = curves[0].GetEndPoint(0); // точка стержня
            XYZ projectUp = ProjectPointOnWorkPlane(plane3D[0], p);
            XYZ projectDown = ProjectPointOnWorkPlane(plane3D[1], p);
            double distUp = p.DistanceTo(projectUp);
            double distDown = p.DistanceTo(projectDown);
            if (distUp < distDown) return AreaLayer.Up;
            else return AreaLayer.Down;
        }

        /// <summary>
        /// Получить поверхности для определения слоя армирования.
        /// </summary>
        /// <remarks>Выбираем поверхности с наибольшей площадью</remarks>
        /// <param name="element">Элемент</param>         
        /// <returns>Список поверхностей: 0 - верхняя 1 - нижняя</returns>

        public static List<Plane3D> GetPlane3DForLayers(Element element)
        {
            XYZ orient = AnalyticalNormal(element);  // получить направление нормали
            if (orient == null) return null;

            List<Face> fnew = new List<Face>();
            List<Face> face = new List<Face>();

            FaceArray faceArray = new FaceArray();
            Options options = new Options();
            options.ComputeReferences = true;

            GeometryElement geomElem = element.get_Geometry(options);
            if (geomElem != null)
            {
                foreach (GeometryObject geomObj in geomElem)
                {
                    GeometryInstance inst = geomObj as GeometryInstance;

                    Solid solid = geomObj as Solid;
                    if (solid != null)
                    {
                        // получить список поверхностей (2 штуки - верх и низ)

                        foreach (Face f in solid.Faces)
                        {
                            fnew.Add(f);
                        }
                    }

                    if (inst != null)
                    {
                        foreach (Object o in inst.SymbolGeometry)
                        {
                            Solid s = o as Solid;
                            if (s != null)
                            {

                                foreach (Face f in s.Faces)
                                {
                                    fnew.Add(f);

                                }
                            }
                        }
                    }
                }
            }


            fnew.Sort(delegate (Face f1, Face f2)
            {
                return f1.Area.CompareTo(f2.Area);
            });


            if (fnew[fnew.Count - 1].ComputeNormal(new UV(0, 0)).AngleTo(orient) < Math.PI / 2)
            {
                face.Add(fnew[fnew.Count - 1]);
                face.Add(fnew[fnew.Count - 2]);
            }
            else
            {
                face.Add(fnew[fnew.Count - 2]);
                face.Add(fnew[fnew.Count - 1]);
            }

            List<Plane3D> plane3Ds = new List<Plane3D>();
            Mesh mesh = face[0].Triangulate();
            plane3Ds.Add(new Plane3D(mesh.Vertices[0], mesh.Vertices[1], mesh.Vertices[3]));
            mesh = face[1].Triangulate();
            plane3Ds.Add(new Plane3D(mesh.Vertices[0], mesh.Vertices[1], mesh.Vertices[3]));
            return plane3Ds;
        }

        public static XYZ TransformPoint(XYZ point, Transform transform)
        {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            //transform basis of the old coordinate system in the new coordinate // system
            XYZ b0 = transform.get_Basis(0);
            XYZ b1 = transform.get_Basis(1);
            XYZ b2 = transform.get_Basis(2);
            XYZ origin = transform.Origin;

            //transform the origin of the old coordinate system in the new 
            //coordinate system
            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
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
        /// Получить проекцию точки на плоскость, заданную тремя точками
        /// </summary>
        /// <param name="PointPlane1">Точка 1 плоскости</param>
        /// <param name="PointPlane2">Точка 2 плоскости</param>
        /// <param name="PointPlane3">Точка 3 плоскости</param>
        /// <param name="p">Проекцируемая точка</param>         
        /// <returns>Точка проекции</returns>
        public static XYZ ProjectPointOnWorkPlane(Plane3D plane3D, XYZ p)
        {
            XYZ a = plane3D.p1 - plane3D.p2;
            XYZ b = plane3D.p1 - plane3D.p3;
            XYZ c = p - plane3D.p1;
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
        /// Получить длину крюка стержня
        /// </summary>

        public static XYZ RoundXYZ(XYZ xyz, int round = 3)
        {
            return new XYZ(Math.Round(xyz.X, round), Math.Round(xyz.Y, 3), Math.Round(xyz.Z, round));
        }
        /// <summary>
        /// Получить длину крюка стержня
        /// </summary>
        /// <returns>Коэффициент шрифта</returns>
        public static double CoeffFont(TextNoteType textNote)
        {
            double shiftUp = 0;
            double shiftDown = 0;
            Font drawFont = new Font(textNote.get_Parameter(BuiltInParameter.TEXT_FONT).AsString(), 48);

            Bitmap temp_font = new Bitmap(100, 100);
            Graphics graphic = Graphics.FromImage(temp_font);
            graphic.Clear(System.Drawing.Color.Transparent);
            graphic.DrawString("0", drawFont, Brushes.Black, 0.0f, 0.0f);

            // найти начало текста по высоте
            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    System.Drawing.Color color = temp_font.GetPixel(x, y);
                    if (color.A > (byte)0)
                    {
                        shiftUp = y;
                        goto ToMaxY;
                    }
                }
            }
        ToMaxY:
            // найти конец текста по высоте
            for (int y = 99; y > 0; y--)
            {
                for (int x = 0; x < 100; x++)
                {
                    System.Drawing.Color color = temp_font.GetPixel(x, y);
                    if (color.A > (byte)0)
                    {
                        shiftDown = y;
                        goto ToMinX;
                    }
                }
            }
        ToMinX:
            return shiftUp / (shiftUp + shiftDown);
        }
        /// <summary>
        /// Получить длину крюка стержня
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="hookId">Id крюка</param>
        /// <returns>Длина крюка</returns>
        public static double GetLengthHook(Rebar rebar, ElementId hookId)
        {
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            RebarBendData rbd = rebar.GetBendData();
            if (hookId.Equals(hook_start)) return rbd.HookLength0;
            if (hookId.Equals(hook_end)) return rbd.HookLength1;
            //IList<Curve> ilc=new List<Curve>();
            //ilc = rebar.GetCenterlineCurves(false, false, false);
            //if (hookId.Equals(hook_start)) return ilc[0].Length;   // длина крюка в начале
            //if (hookId.Equals(hook_end)) return ilc.Last().Length;   // длина крюка в начале
            return 0;
        }

        /// <summary>
        /// Получить длину крюка стержня
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="hookId">Id крюка</param>
        /// <returns>Длина крюка</returns>
        public static double GetLengthHook(RebarInSystem rebar, ElementId hookId)
        {
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            RebarBendData rbd = rebar.GetBendData();
            if (hookId.Equals(hook_start)) return rbd.HookLength0;
            if (hookId.Equals(hook_end)) return rbd.HookLength1;
            return 0;
        }

        /// <summary>
        /// Минимальное значение при сравнении
        /// </summary>
        public const double Double_Epsilon = 0.01;
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, BuiltInParameter guid, ElementId value)
        {

            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Rebar fi, BuiltInParameter guid, ElementId value)
        {

            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, BuiltInParameter guid, double value)
        {
            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, BuiltInParameter guid, string value)
        {
            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }

        public static bool CompareXYZ(Autodesk.Revit.DB.XYZ pnt1, Autodesk.Revit.DB.XYZ pnt2)
        {
            return (CompareDouble(pnt1.X, pnt2.X) &&
                    CompareDouble(pnt1.Y, pnt2.Y) &&
                    CompareDouble(pnt1.Z, pnt2.Z));

        }
        /// <summary>
        /// compare whether 2 double is equal using internal precision
        /// </summary>
        /// <param name="d1">Первое значение</param>
        /// <param name="d2">Второе значение</param>
        /// <returns>Да если A=B</returns>
        public static bool CompareDouble(double d1, double d2)
        {
            return (Math.Abs(d1 - d2) < Double_Epsilon && (Math.Abs(d2) > Double_Epsilon ? d1 / d2 > 0 : true));
        }

        /// <summary>
        /// compare whether 2 double is equal using internal precision
        /// </summary>
        /// <param name="d1">Первое значение</param>
        /// <param name="d2">Второе значение</param>
        /// <returns>Да если A>B</returns>
        public static bool CompareDoubleMore(double d1, double d2)
        {
            return (d1 - d2 > Double_Epsilon ? true : false);
        }

        /// <summary>
        /// Получить координаты крайних точек из списка 
        /// </summary>
        /// <param name="pointDF">Список точек</param>
        /// <returns>Координаты крайних точек</returns> 
        /// 
        public static void GetExtremePoints(IList<Curve> Curves, out double minX, out double minY, out double maxX, out double maxY)
        {

            List<XYZ> arc_points = new List<XYZ>();
            // получаем все точки кривых стержня
            foreach (Curve curve in Curves)
            {
                arc_points.Add(curve.GetEndPoint(0));
                arc_points.Add(curve.GetEndPoint(1));

                Arc arc = curve as Arc;
                if (arc != null)
                {
                    foreach (XYZ p in arc.Tessellate())
                    {
                        arc_points.Add(p);
                    }
                }
            }

            minX = maxX = arc_points[0].X;
            minY = maxY = arc_points[0].Y;

            for (int i = 0; i < arc_points.Count(); i++)
            {
                double fx = arc_points[i].X;
                double fy = arc_points[i].Y;
                minX = Math.Min(minX, fx);
                maxX = Math.Max(maxX, fx);
                minY = Math.Min(minY, fy);
                maxY = Math.Max(maxY, fy);
            }
        }


        /// <summary>
        /// Получить округленную длину сегмента. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="length_segment">Длина стержня в единицах Revit</param>
        /// <returns>Округленное строковое значение</returns>       
        public static string GetRoundLenghtRebar(Element rebar, double length_segment)
        {
            // длину стержня окрругляем до 1 мм во всех случаях
            length_segment = Math.Round(length_segment * 304.8, 0) / 304.8;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return length_segment.ToString();

            double precision = rrm.ApplicableTotalLengthRounding;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                length_segment = length_segment * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();


                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }

                //FormatOptions formatOption = projectUnit.GetFormatOptions(UnitType.UT_Reinforcement_Length);
                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableTotalLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = length_segment;
            else round_value = Math.Round(length_segment / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            return UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value, false);
        }



        /// <summary>
        /// Получить округленную длину сегмента. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="length_segment">Длина сегмента в единицах Revit</param>
        /// <returns>Округленное строковое значение</returns>       
        public static string GetRoundLenghtSegment(Element rebar, double length_segment)
        {
            // длину стержня окрругляем до 1 мм во всех случаях
            length_segment = Math.Round(length_segment * 304.8, 0) / 304.8;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return length_segment.ToString();

            double precision = rrm.ApplicableSegmentLengthRounding;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                length_segment = length_segment * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();


                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }
                //FormatOptions formatOption = projectUnit.GetFormatOptions(UnitType.UT_Reinforcement_Length);
                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableSegmentLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = length_segment;
            else round_value = Math.Round(length_segment / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            return UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value, false);
        }


        /// <summary>
        /// Получить диаметр стержня. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <returns>Округленное строковое значение</returns>       
        public static string GetRoundDiametrRebar(Element rebar)
        {
            double Diametr = 0;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            if (rebarOne != null)
            {

                Diametr = Math.Round(rebarOne.GetBendData().BarModelDiameter, 3);
            }
            else
            {

                Diametr = Math.Round(rebarIn.GetBendData().BarModelDiameter, 3);
            }

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return Diametr.ToString();

            FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.BarDiameter);
            double precision = formatOption.Accuracy;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                Diametr = Diametr * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.BarDiameter, precision.ToString(), out unit);
            }
            else
            {

                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();

                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.BarDiameter, precision.ToString(), out unit);
                }

                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;                
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Bar_Diameter, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableTotalLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = Diametr;
            else round_value = Math.Round(Diametr / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            return UnitFormatUtils.Format(projectUnit, SpecTypeId.BarDiameter, round_value, false);
        }

        /// <summary>
        /// Получить полную длину стержня. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <returns>Округленное строковое значение</returns>       
        public static string GetRoundFullLengthRebar(Element rebar)
        {

            double Length_max = 0;
            double Length_min = 0;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            if (rebarOne != null)
            {

                Length_max = rebarOne.get_Parameter(BuiltInParameter.REBAR_MAX_LENGTH).AsDouble();
                Length_min = rebarOne.get_Parameter(BuiltInParameter.REBAR_MIN_LENGTH).AsDouble();
            }
            else
            {

                Length_max = rebarIn.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble();
                Length_min = rebarIn.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble();
            }

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return Math.Max(Length_max, Length_min).ToString();

            double precision = rrm.ApplicableTotalLengthRounding;

            // длину стержня окрругляем до 1 мм во всех случаях
            Length_max = Math.Round(Length_max * 304.8, 0) / 304.8;
            Length_min = Math.Round(Length_min * 304.8, 0) / 304.8;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                Length_max = Length_max * 12;  // все перевести в десятичные дюймы
                Length_min = Length_min * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();


                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }

                //FormatOptions formatOption = projectUnit.GetFormatOptions(UnitType.UT_Reinforcement_Length);
                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableTotalLengthRounding;
            double round_value_max = 0;
            double round_value_min = 0;
            if (unit == 0)
            {
                round_value_max = Length_max;
                round_value_min = Length_min;
            }
            else
            {
                round_value_max = Math.Round(Length_max / unit, 0) * unit;
                round_value_min = Math.Round(Length_min / unit, 0) * unit;
            }
            if (du == DisplayUnit.IMPERIAL)
            {
                round_value_max = round_value_max / 12;                       // перевести в десятичные футы
                round_value_min = round_value_min / 12;
            }
            string S = UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value_max, false);
            if (Length_max != Length_min)
            {
                S = S + "..." + UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value_min, false);
            }
            return S;
        }

        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, Guid guid, double value)
        {
            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }


        /// <summary>
        /// Данные по участкам армирования
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>         
        /// <returns>Данные по сегментам стержня</returns>
        public static string DataBySegments(Element element)
        {

            TextOnRebar tor = new TextOnRebar();
            tor.rebar = element;

            Document doc = element.Document;
            Rebar rebarOne = element as Rebar;
            RebarInSystem rebarIn = element as RebarInSystem;
            RebarShape rs = null;
            string segments = "";

            // здесь выполняем 
            if (rebarOne != null)
            {

                rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;

            }
            if (rebarIn != null)
            {

                // получить данные по форме стержня
                rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;


            }

            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
            ParameterSet pset = element.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 

            if (rsds != null)
            {
                // Цикл по сегментам в данной форме rsds.NumberOfSegments
                for (int i = 0; i < rsds.NumberOfSegments; i++)
                {
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

                                    tor.value = element.get_Parameter(pr.Definition).AsDouble();
                                    // segments = segments + Math.Round(element.get_Parameter(pr.Definition).AsDouble(), 2) + " ;";                                   
                                    if (tor.value > 0) segments = segments + tor.value_str + " ;";

                                }
                            }
                        }
                    }
                }
            }



            tor.value = element.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble();
            return segments = segments + tor.value_str_total;

        }

        /// <summary>
        /// Создать дубликат стержня для свободной формы по траектории на ТЕКУЩЕМ ВИДЕ
        /// </summary>
        /// <param name="rebar">Арматурный стержень как FreeForm</param>  
        /// <param name="view">Вид для размещения</param> 
        /// <returns>Да, если успешно создан</returns>
        public static Element CreateDublicateRebar(Element element, View view)
        {
            Rebar rebar = element as Rebar;
            if (rebar == null) return element;
            if (!rebar.IsRebarFreeForm()) return element;
            Document doc = rebar.Document;
            RebarShape rebarShape = doc.GetElement(rebar.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()) as RebarShape;
            RebarBarType barType = doc.GetElement(rebar.GetTypeId()) as RebarBarType;
            Element host = doc.GetElement(rebar.GetHostId()) as Element;
            BoundingBoxXYZ boundingBox = host.get_BoundingBox(view);
            XYZ origin = (boundingBox.Max + boundingBox.Min) / 2;
            XYZ xVec = view.RightDirection;
            XYZ yVec = view.UpDirection;
            XYZ zVec = view.ViewDirection;
            // Rebar new_rebar = Rebar.CreateFromRebarShape(doc, rebarShape, barType, host, origin, xVec,yVec);
            RebarHookType startHook = doc.GetElement(rebar.GetHookTypeId(0)) as RebarHookType;
            RebarHookType endHook = doc.GetElement(rebar.GetHookTypeId(1)) as RebarHookType;
            // получить данные по форме стержня
            IList<Curve> ilc = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
            RebarHookOrientation startHookOrient = rebar.GetHookOrientation(0);
            RebarHookOrientation endHookOrient = rebar.GetHookOrientation(1);
            Rebar new_rebar = Rebar.CreateFromCurvesAndShape(doc, rebarShape, barType, startHook, endHook, host, zVec, ilc, startHookOrient, endHookOrient);
            return new_rebar as Element;
        }

        /// <summary>
        /// Получить вектор-нормаль к плоскости заданной линиями
        /// </summary>
        /// <param name="curves">Список кривых</param>          
        /// <returns>Вектор-нормаль</returns>
        public static XYZ GetNormalVector(IList<Curve> curves)
        {
            XYZ p1, p2, p3;
            if (curves == null) return XYZ.Zero;
            if (curves.Count == 0) return XYZ.Zero;
            p1 = curves[0].GetEndPoint(1);
            p2 = curves[0].GetEndPoint(0);
            if (curves.Count > 1)
            {
                p3 = curves[curves.Count - 1].GetEndPoint(1);
            }
            else
            {
                // один отрезок. Нормаль определяем произвольно.                
                p3 = (p1 - p2).Normalize();
                p3 = new XYZ(p3.X + 1, p3.Y + 1, p3.Z + 1);
            }
            XYZ v1 = (p2 - p1).Normalize();
            XYZ v2 = (p3 - p1).Normalize();
            return v1.CrossProduct(v2);
        }
    }

        /// <summary>
        /// Надписи над прямыми участками стержней
        /// </summary>
        public class TextOnRebar : IEquatable<TextOnRebar>
        {
            ///// <summary>
            ///// Признак отображения в диалоге
            ///// </summary>
            //public bool dialog = true;
            ///// <summary>
            ///// Множитель значения
            ///// </summary>
            //public double coeff = 1;
            ///// <summary>
            ///// Повторный размер
            ///// </summary>
            //public bool repeat = false;
            ///// <summary>
            ///// Размер надписи
            ///// </summary>
            //public SizeF size;
            /// <summary>
            /// Стержень для которого выполняется надпись
            /// </summary>
            public Element rebar = null;
            /// <summary>
            /// Guid параметра
            /// </summary>
            public Guid guid;
            ///// <summary>
            ///// Guid параметра (проекция вертикальная)
            ///// </summary>
            //public Guid guidV;
            ///// <summary>
            ///// Guid параметра (проекция горизонтальная)
            ///// </summary>
            //public Guid guidH;
            /// <summary>
            /// Начальная точка сегмента
            /// </summary>
            public XYZ start;
            /// <summary>
            /// Начальная точка сегмента - начальная
            /// </summary>
            public XYZ start_initial;
            /// <summary>
            /// Направление сегмента стержня - начальное
            /// </summary>
            public XYZ dir_segment_initial
            {
                get
                {
                    return (end_initial - start_initial).Normalize();
                }

            }
            /// <summary>
            /// Направление сегмента стержня
            /// </summary>
            public XYZ dir_segment
            {
                get
                {
                    return (end - start).Normalize();
                }

            }
            /// <summary>
            /// Конечная точка сегмента
            /// </summary>
            public XYZ end;
            /// <summary>
            /// Конечная точка сегмента - начальная
            /// </summary>
            public XYZ end_initial;
            /// <summary>
            /// Начальная точка сегмента
            /// </summary>
            public PointF startF;
            /// <summary>
            /// Конечная точка сегмента
            /// </summary>
            public PointF endF;
            ///// <summary>
            ///// Расстояние между точками
            ///// </summary>
            //public float distF
            //{
            //    get { return (float)(Math.Sqrt(Math.Pow((startF.X - endF.X), 2) + Math.Pow((startF.Y - endF.Y), 2))); }
            //}
            /// <summary>
            /// Позиция параметра
            /// </summary>
            public XYZ position;
            /// <summary>
            /// Позиция параметра
            /// </summary>
            public PointF positionF;
            ///// <summary>
            ///// Позиция параметра после возможного поворота
            ///// </summary>
            //public PointF positionF_rotate
            //{
            //    get
            //    {
            //        // координаты новой точки после поворота на угол "angle_rotate"
            //        float X_new = (float)(positionF.X * Math.Cos(angle) + positionF.Y * Math.Sin(angle));
            //        float Y_new = (float)(-positionF.X * Math.Sin(angle) + positionF.Y * Math.Cos(angle));
            //        // сдвиг на длину надписи 
            //        // return new PointF(X_new - size.Width / 2, Y_new);
            //        return new PointF(X_new, Y_new);
            //    }
            //}
            ///// <summary>
            ///// Значение параметра в эскизе
            ///// </summary>
            //public double value_sketch = 0; 
            /// <summary>
            /// Значение параметра
            /// </summary>
            public double value = 0;
            /// <summary>
            /// Значение параметра минимальное (для стержней переменной длины)
            /// </summary>
            public double value_min = 0;

            /// <summary>
            /// Значение параметра начальное
            /// </summary>
            public double value_initial = 0;

            /// <summary>
            /// Округленное строковое значение параметра
            /// </summary>
            public string value_str
            {
                get
                {
                    string v = SketchTools.GetRoundLenghtSegment(rebar, value);
                    if (v.Length < 2) return v;
                    if (v.Substring(0, 2) == "0.") v = v.Substring(1);
                    if (value_min > 0)
                    {
                        string smin = SketchTools.GetRoundLenghtSegment(rebar, value_min);
                        if (smin.Length < 2) return smin;
                        if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                        v = v + "..." + smin;
                    }
                    return v;
                }
            }

            /// <summary>
            /// Округленное строковое значение параметра (для всей длины стержня)
            /// </summary>
            public string value_str_total
            {
                get
                {
                    string v = SketchTools.GetRoundLenghtRebar(rebar, value);
                    if (v.Length < 2) return v;
                    if (v.Substring(0, 2) == "0.") v = v.Substring(1);
                    return v;
                }
            }

            /// <summary>
            /// Имя параметра
            /// </summary>
            public string name = "";
            ///// <summary>
            ///// Значение параметра
            ///// </summary>
            //public double valueV = 0;
            ///// <summary>
            ///// Значение параметра
            ///// </summary>
            //public string valueV_str
            //{
            //    get
            //    {
            //        string v=SketchTools.GetRoundLenghtSegment(rebar, valueV);
            //        if (v.Length < 2) return v;
            //        if (v.Substring(0, 2) == "0.") v = v.Substring(1);
            //        return v;
            //    }
            //}

            ///// <summary>
            ///// Имя параметра
            ///// </summary>
            //public string nameV = "";
            ///// <summary>
            ///// Значение параметра
            ///// </summary>
            //public double valueH = 0;
            ///// <summary>
            ///// Значение параметра
            ///// </summary>
            //public string valueH_str
            //{
            //    get
            //    {
            //        string v = SketchTools.GetRoundLenghtSegment(rebar, valueH);
            //        if (v.Length < 2) return v;
            //        if (v.Substring(0, 2) == "0.") v = v.Substring(1);
            //        return v;
            //    }
            //}
            ///// <summary>
            ///// Имя параметра
            ///// </summary>
            //public string nameH = "";
            /// <summary>
            /// Признак дуги (арки)
            /// </summary>
            public bool arc = false;
            /// <summary>
            /// Признак наличия любого крюка
            /// </summary>
            public bool isHook
            {
                get
                {
                    if (isHookStart || isHookStart) return true;
                    return false;
                }
            }
            /// <summary>
            /// Признак наличия начального крюка
            /// </summary>
            public bool isHookStart = false;
            /// <summary>
            /// Признак наличия конечного крюка
            /// </summary>
            public bool isHookEnd = false;
            ///// <summary>
            ///// Угол наклона надписи в градусах
            ///// </summary>
            //public float angle_grad
            //{
            //    get { return (float)(180 / Math.PI * angle); }
            //}
            /// <summary>
            /// Угол наклона надписи
            /// </summary>
            public double angle = 0;

            /// <summary>
            /// Получить угол наклона надписи по направлениям вида 
            /// </summary>
            /// <param name="dirSegment">Направление сегмента в общей системе координат</param>
            /// <param name="dirRight">Направление на чертеже вправо</param>
            /// <param name="dirUp">Направление на чертеже вверх</param>
            /// <returns>Угол наклона надписи</returns>
            public double GetAngleForTextNote(XYZ dirSegment, XYZ dirRight, XYZ dirUp)
            {
                double angle = Math.Round(dirRight.AngleTo(dirSegment), 3);
                double beta = Math.Round(dirUp.AngleTo(dirSegment), 3);

                if (angle > 1.571 && beta <= 1.571)
                {
                    return -(3.142 - angle);      // 2 четверть                
                }

                if (beta > 1.571)
                {
                    if (angle >= 1.571)
                    {
                        return (3.142 - angle);  // 3 четверть

                    }
                    else
                        return -angle;           // 4 четверть                                 
                }
                return angle;
            }
            /// <summary>
            /// Угол наклона надписи в первой четверти
            /// </summary>
            public double angleI
            {
                get
                {
                    double a = Math.Round(angle, 3);
                    //if (a == 0 || a == 3.142) return 0;
                    //XYZ dir = dir_segment.Normalize();
                    //if (dir.X < 0 && dir.Y > 0) return a; // - Math.PI + ;  // 2 четверть
                    //if (dir.X > 0 && dir.Y < 0) return - a;          // 4 четверть
                    //if (dir.X < 0 && dir.Y < 0) return Math.PI - a;  // 3 четверть               
                    return a;
                }
            }

            ///// <summary>
            ///// Признак наклона надписи
            ///// </summary>
            //public InclineText incline = InclineText.Horiz;       // по умолчанию
            ///// <summary>
            ///// Получить координаты с учетом масштаба
            ///// </summary>
            //public void UsingScale(float scale)
            //{
            //    positionF = new PointF(positionF.X * scale, positionF.Y * scale);
            //    startF = new PointF(startF.X * scale, startF.Y * scale);
            //    endF = new PointF(endF.X * scale, endF.Y * scale);
            //}

            public bool Equals(TextOnRebar other)
            {

                //Check whether the compared object is null.
                if (Object.ReferenceEquals(other, null)) return false;

                //Check whether the compared object references the same data.
                if (Object.ReferenceEquals(this, other)) return true;

                //Check whether the products' properties are equal.
                // return name.Equals(other.name) || nameV.Equals(other.nameV) || nameH.Equals(other.nameH);
                return name.Equals(other.name);
            }


            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.

            public override int GetHashCode()
            {

                //Get hash code for the Name field if it is not null.
                int hashProductGost = name == null ? 0 : name.GetHashCode();

                //int hashProductClass = nameV == null ? 0 : nameV.GetHashCode();

                //int hashProductBar = nameH == null ? 0 : nameH.GetHashCode();

                //Calculate the hash code for the product.
                return hashProductGost;  // ^ hashProductClass ^ hashProductBar;
            }


        }



        ///// <summary>
        ///// Надписи над стержнями 
        ///// </summary>
        //public class TextOnArc
        //{
        //    /// <summary>
        //    /// Размер надписи
        //    /// </summary>
        //    public SizeF size;
        //    /// <summary>
        //    /// Стержень для которого выполняется надпись
        //    /// </summary>
        //    public Element rebar;  
        //    /// <summary>
        //    /// Начальная точка сегмента
        //    /// </summary>
        //    public XYZ start;
        //    /// <summary>
        //    /// Конечная точка сегмента
        //    /// </summary>
        //    public XYZ end;
        //    /// <summary>
        //    /// Начальная точка сегмента
        //    /// </summary>
        //    public PointF startF;
        //    /// <summary>
        //    /// Конечная точка сегмента
        //    /// </summary>
        //    public PointF endF; 
        //    /// <summary>
        //    /// Позиция параметра
        //    /// </summary>
        //    public XYZ position;
        //    /// <summary>
        //    /// Позиция параметра
        //    /// </summary>
        //    public PointF positionF;
        //    /// <summary>
        //    /// Позиция параметра после возможного поворота
        //    /// </summary>
        //    public PointF positionF_rotate
        //    {
        //        get
        //        {
        //            // координаты новой точки после поворота на угол "angle_rotate"
        //            float X_new = (float)(positionF.X * Math.Cos(angle) + positionF.Y * Math.Sin(angle));
        //            float Y_new = (float)(-positionF.X * Math.Sin(angle) + positionF.Y * Math.Cos(angle));                 
        //            return new PointF(X_new, Y_new);

        //        }
        //    }
        //    /// <summary>
        //    /// Длина примыкающих прямых участков
        //    /// </summary>
        //    public double nearestL = 0;
        //    /// <summary>
        //    /// Значение параметра
        //    /// </summary>
        //    public double value = 0;
        //    /// <summary>
        //    /// Округленное строковое значение параметра
        //    /// </summary>
        //    public string value_str
        //    {
        //        get
        //        {
        //            string v = SketchTools.GetRoundLenghtSegment(rebar, value);
        //            if (v.Length > 2)
        //            {
        //                if (v.Substring(0, 2) == "0.") v = v.Substring(1);
        //            }
        //            return v;
        //        }
        //    }

        //    /// <summary>
        //    /// Признак дуги (арки)
        //    /// </summary>
        //    public bool arc = true;
        //    /// <summary>
        //    /// Угол наклона надписи в градусах
        //    /// </summary>
        //    public float angle_grad
        //    {
        //        get
        //        {
        //            return (float)(180 / Math.PI * angle);
        //        }
        //    }
        //    /// <summary>
        //    /// Угол наклона надписи
        //    /// </summary>
        //    public float angle = 0;
        //    /// <summary>
        //    /// Признак наклона надписи
        //    /// </summary>
        //    public InclineText incline = InclineText.Incline;       // по умолчанию

        //    /// <summary>
        //    /// Получить координаты с учетом масштаба
        //    /// </summary>
        //    public void UsingScale(float scale)
        //    {
        //        positionF = new PointF(positionF.X * scale, positionF.Y * scale);
        //        startF = new PointF(startF.X * scale, startF.Y * scale);
        //        endF = new PointF(endF.X * scale, endF.Y * scale);
        //    }


        //}



    }


