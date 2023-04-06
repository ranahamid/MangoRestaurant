namespace Mango.Web.Extensions
{
    public static class ObjectExtensions
    {
        public static T ConvertNullPropertiesToDefault<T>(this T obj)
        {
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property.GetValue(obj) == null)
                {
                    Type propertyType = property.PropertyType;
                    var name = propertyType.Name.ToLower();
                    object defaultValue = name == "string"? "": name=="datetime"? DateTime.UtcNow:null;

                    //object defaultValue = default(propertyType);
                     
                    property.SetValue(obj, defaultValue);
                }
            }

            return obj;
        }
    }
}
