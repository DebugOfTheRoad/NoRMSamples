namespace NoRMSamples
{
    public abstract class Entity<T> where T : Entity<T>
    {

        public int Id { get; set; }
      

        /// <summary>
        /// Indicates whether the current <see cref="T:FluentNHibernate.Data.Entity" /> is equal to another <see cref="T:FluentNHibernate.Data.Entity" />.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="obj" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="obj">An Entity to compare with this object.</param>
        public virtual bool Equals(Entity<T> obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.Id == Id;
        }

        /// <summary>
        /// Determines whether the specified  entity is equal to the current
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:FluentNHibernate.Data.Entity" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj" /> parameter is null.</exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj as T == null)
            {
                return false;
            }
            return Equals((Entity<T>)obj);
        }

        /// <summary>
        /// Serves as a hash function for a Entity.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (Id.GetHashCode() * 397) ^ GetType().GetHashCode();
        }

        public static bool operator ==(Entity<T> left, Entity<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Entity<T> left, Entity<T> right)
        {
            return !Equals(left, right);
        }
    }
}