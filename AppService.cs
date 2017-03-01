using System;

namespace AppServiceDomainChecker
{
    public class AppService
    {
        private bool isASE;
        private bool usingTM;
        private string aseName;
        private string tmName;
        private string appServiceName;

        public bool IsASE
        {
            get
            {
                return isASE;
            }
            set
            {
                isASE = value;
            }
        }

        public bool UsingTM
        {
            get
            {
                return usingTM;
            }
            set
            {
                usingTM = value;
            }
        }

        public string AseName
        {
            get
            {
                return aseName;
            }
            set
            {
                aseName = value;
            }
        }

        public string TmName
        {
            get
            {
                return tmName;
            }
            set
            {
                tmName = value;
            }
        }

        public string AppServiceName
        {
            get
            {
                return appServiceName;
            }
            set
            {
                appServiceName = value;
            }
        }
    }
}
