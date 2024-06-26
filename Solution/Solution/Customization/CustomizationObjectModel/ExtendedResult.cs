﻿using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.ObjectModel
{
    [SampleManagerEntity(Result.EntityName)]
    public class ExtendedResult : Result
    {
        public bool Processed { get; set; }
        protected override void OnPropertyChanged(PropertyEventArgs e)
        {
            base.OnPropertyChanged(e);

            //ModifiedDate = System.DateTime.Now;
        }
    }
}
