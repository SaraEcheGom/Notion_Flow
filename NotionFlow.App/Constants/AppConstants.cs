namespace NotionFlow.App.Constants
{
    /// <summary>Roles de usuario normalizados. Deben coincidir exactamente con los roles del backend.</summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Professor = "Professor";
        public const string Student = "Student";
    }

    /// <summary>Valores de tipo de pregunta intercambiados con la API.</summary>
    public static class QuestionTypes
    {
        public const string MultipleChoice = "MultipleChoice";
        public const string OpenText = "OpenText";
    }

    /// <summary>Rutas de navegación Shell registradas en AppShell.</summary>
    public static class Routes
    {
        public const string Register = "register";
        public const string Login = "//login";

        public const string AdminHome = "//admin_home";
        public const string TeacherHome = "//teacher_home";
        public const string StudentHome = "//student_home";

        public const string AdminCreateCourse = "admin/create-course";
        public const string AdminCreateTeacher = "admin/create-teacher";
        public const string AdminCreateStudent = "admin/create-student";
        public const string AdminCourseDetail = "admin/course-detail";

        public const string TeacherCourseDetail = "teacher/course-detail";
        public const string TeacherCreateActivity = "teacher/create-activity";
        public const string TeacherEditActivity = "teacher/edit-activity";
        public const string TeacherAssignActivity = "teacher/assign-activity";
        public const string TeacherPublishContent = "teacher/publish-content";
        public const string TeacherCreateEval = "teacher/create-eval";
        public const string TeacherActivityResults = "teacher/activity-results";
        public const string TeacherGenerateQuiz = "teacher/generate-quiz";

        public const string StudentCourseDetail = "student/course-detail";
        public const string StudentTakeActivity = "student/take-activity";
    }
}
