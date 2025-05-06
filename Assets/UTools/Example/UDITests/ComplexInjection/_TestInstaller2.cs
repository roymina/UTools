using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Example
{
    public class _TestInstaller2 : UDIInstallerBase
    {
        protected override void RegisterServices()
        {
            Container.Register<_TestDataEntity>();
             
        }
    }
}

