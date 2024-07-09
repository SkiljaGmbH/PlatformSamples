using System.ComponentModel;
using System.IO;
using System.Windows.Documents;
using System.Windows.Input;
using EDAClient.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDAClient.ViewModels
{
    public class ResultViewModel : INotifyPropertyChanged
    {
        private ICommand _openResultCommand;
        private readonly string _outputPath;


        public string HeadlineText { get; } = $"{SharedData.OEM.Process} Execution";

        public ResultViewModel(string outputPath)
        {
            _outputPath = outputPath;
            var json = File.ReadAllText(Path.Combine(outputPath, "Export.json"));
            json = JToken.Parse(json).ToString(Formatting.Indented);
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(json);
            JsonFD = new FlowDocument(paragraph);
        }

        public FlowDocument JsonFD { get; set; }

        public ICommand OpenResultCommand =>
            _openResultCommand ?? (_openResultCommand = new CommandExecutor(openResultCommand_Execute));

        public event PropertyChangedEventHandler PropertyChanged;

        private void openResultCommand_Execute(object o)
        {
            SharedData.IOService.OpenFolderInFS(_outputPath);
        }
    }
}