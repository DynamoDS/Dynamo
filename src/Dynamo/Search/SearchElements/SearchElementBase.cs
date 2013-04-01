namespace Dynamo.Search
{
    public abstract class SearchElementBase
    {
        public abstract string Type { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract double Weight { get; set; }
        public abstract void Execute();

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var ele = (SearchElementBase) obj;
            return this.Type == ele.Type && this.Name == ele.Name && this.Description == ele.Description;

        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + this.Name.GetHashCode() + this.Description.GetHashCode();
        }
    }
}
