using L5XAutomationTool.GUIAccessors;

namespace L5XAutomationTool
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Compose services
            IValidationService validation = new ValidationService();
            IMessageService messages = new MessageService();
            IUserPromptService prompts = new UserPromptService();
            IOpenFileService openFile = new OpenFileService();
            ISaveFileService saveFile = new SaveFileService();
            IFileSystem fs = new FileSystem();
            ISchemaDisambiguator disam = new SchemaDisambiguator(prompts);

            var xml = new XMLHandler(validation, disam);
            var app = new Execute(xml, messages, prompts, openFile, saveFile, validation, fs);
            Execute.InitializeNew(); // same behavior as your previous startup default

            // pass the instance "app" into the form so it can call instance methods.
            Application.Run(new Forms.ActionSelect(app));
        }
    }
}
