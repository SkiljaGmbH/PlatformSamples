using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ActivitySettings.Configuration;
using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;


namespace ActivitySettings
{
    /// <summary>
    /// External activity with the ExternalSettings's properties
    /// </summary>
    public class ExternalActivityWithAllConfiguationSettings : STGExternalAbstract<ExternalSettings> { }
    
}
