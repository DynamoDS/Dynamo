namespace ProtoScript.Config
{
    public struct RunConfiguration
    {
        private bool isParrallel;

        public bool IsParrallel {
            get
            {
                return isParrallel;
            }

            set
            {
                isParrallel = value;
            }
        }


    }
}
