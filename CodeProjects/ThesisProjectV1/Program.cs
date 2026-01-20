using System;
using System.Windows.Forms;
using ThesisProjectV1.Abstractions;
using ThesisProjectV1.Infrastructure;
using ThesisProjectV1.WinFormsAdapters;

namespace ThesisProjectV1
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

            var xml = new XMLHandler(validation);
            var app = new Execute(xml, messages, prompts, openFile, saveFile, validation, fs);
            app.InitializeNew(); // same behavior as your previous startup default

            // pass the instance "app" into the form so it can call instance methods.
            Application.Run(new Forms.ActionSelect(app));
        }
    }
}
