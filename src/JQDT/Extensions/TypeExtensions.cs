﻿namespace JQDT.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Extension methods for <see cref="Type"/>
    /// </summary>
    public static class TypeExptensions
    {
        /// <summary>
        /// Returns collection of <see cref="PropertyInfo"/>. The collection contains the <see cref="PropertyInfo"/> of
        /// the model properties from the parent properties to the target property.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="propertyName">
        /// Name of the property. May contain nested properties delimited by ".".
        /// Example: Address.Street.Number
        /// </param>
        /// <returns><see cref="ICollection{PropertyInfo}"/></returns>
        /// <exception cref="ArgumentException">Thrown on invalid property name.</exception>
        public static ICollection<PropertyInfo> GetPropertyInfoPath(this Type model, string propertyName)
        {
            var propertyNamePath = propertyName.Split('.');
            Type currentModelType = model;
            var propertyInfoPath = new List<PropertyInfo>();
            foreach (var propName in propertyNamePath)
            {
                var propInfo = currentModelType.GetProperties().FirstOrDefault(p => p.Name == propName);
                if (propInfo == null)
                {
                    throw new ArgumentException(
                        $@"Invalid property name. The property {propertyName} does not exist in the model.
                         Make sure that the data property of the column is configured appropriately as described
                        in jQuery Datatables documentation: https://datatables.net/examples/ajax/objects.html");
                }

                currentModelType = propInfo.PropertyType;
                propertyInfoPath.Add(propInfo);
            }

            return propertyInfoPath;
        }

        /// <summary>
        /// Gets the property select expression.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="propertyInfoPath">Collection of <see cref="PropertyInfo"/> describing the path from parent to target property.
        /// </param>
        /// <returns><see cref="LambdaExpression"/> Ex: "x => (ModelType)x.Property1.Property2"</returns>
        public static LambdaExpression GetPropertySelectExpression(this Type modelType, IEnumerable<PropertyInfo> propertyInfoPath)
        {
            // x
            ParameterExpression xExpr = Expression.Parameter(typeof(object), "x");

            // (ModelType)x
            var castExpr = Expression.Convert(xExpr, modelType);

            // (ModelType)x.Property
            MemberExpression propExpression = null;
            foreach (var propInfo in propertyInfoPath)
            {
                propExpression = propExpression == null ?
                    Expression.Property(castExpr, propInfo) :
                    Expression.Property(propExpression, propInfo);
            }

            // x => (ModelType)x.Property
            var lambda = Expression.Lambda(propExpression, xExpr);

            return lambda;
        }

        /// <summary>
        /// Determines whether the specified type is a System type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is CLR library type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCLRLibraryType(this Type type)
        {
            return type.Module.ScopeName != "CommonLanguageRuntimeLibrary";
        }
    }
}