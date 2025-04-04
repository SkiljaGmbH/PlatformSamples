using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleEventDrivenActivity.Configuration
{
    public class ClassificationServiceData : ActivityConfigBase<ClassificationServiceData>
    {

        public ClassificationServiceData()
        {
            DocClassificationProjectCVName = "CV_ClassificationProjectName";
            PageClassificationProjectCVName = "CV_SegmentationProjectName";
        }

        [Display(Name = "Classification service URL", Description = "This property provides an URL where the classification service is running.", Order = 1)]
        [InputType(InputType.url)]
        public string ClassificationServiceURL { get; set; }


        [Display(Name = "Contains Document Classifier", Description = "If set, it is required to fill the custom value defined below with the process name for document classification.", Order = 2)]
        [InputType(InputType.checkbox)]
        public bool HaveDocClassifier { get; set; }

        [Display(Name = "Document Classifier Project Custom Value", Description = "This property provides a custom value name that contains a name of the classification project the document classifier activity will use", Order = 3)]
        [InputType(InputType.text)]
        public string DocClassificationProjectCVName { get; set; }

        [Display(Name = "Contains Page Classifier", Description = "If set, it is required to fill the custom value defined below with the process name for page classification and document separation project.", Order = 4)]
        [InputType(InputType.checkbox)]
        public bool HavePageClassifier { get; set; }

        [Display(Name = "Page Clessifier Project Custom Value", Description = "This property provides a custom value name that contains a name of the classification project the page classifier activity will use", Order = 5)]
        [InputType(InputType.text)]
        public string PageClassificationProjectCVName { get; set; }

    }
}
