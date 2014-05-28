using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa.Routing
{
    public abstract class AbstractRoute
    {
        private class Part
        {
            public object defaultValue;
            public string pattern;
            public Type type;
            public int order;
            public bool optional;

            public virtual bool Match(string part)
            {
                return part == pattern;
            }

            public virtual void Parse(string part, object[] parameters)
            {
            }

            public virtual void SetDefault(object[] parameters)
            {
                parameters[order] = defaultValue;
            }

            public virtual void Append(StringBuilder builder, object parameters)
            {
                builder.Append(pattern);
            }
        }

        private class BoolPart : Part
        {
            public BoolPart()
            {
                type = typeof(bool);
            }

            public override bool Match(string part)
            {
                bool result;
                if (!bool.TryParse(part, out result))
                    return false;
                return true;
            }

            public override void Parse(string part, object[] parameters)
            {
                parameters[order] = bool.Parse(part);
            }

            public override void Append(StringBuilder builder, object parameters)
            {
                var type = parameters.GetType();
                var value = type.GetProperty(pattern).GetValue(parameters);
                builder.Append((bool)value);
            }
        }

        private class IntPart : Part
        {
            public IntPart()
            {
                type = typeof(int);
            }

            public override bool Match(string part)
            {
                int result;
                if (!int.TryParse(part, out result))
                    return false;
                return true;
            }

            public override void Parse(string part, object[] parameters)
            {
                parameters[order] = int.Parse(part);
            }
            
            public override void Append(StringBuilder builder, object parameters)
            {
                var type = parameters.GetType();
                var value = type.GetProperty(pattern).GetValue(parameters);
                builder.Append((int)value);
            }
        }

        private class LongPart : Part
        {
            public LongPart()
            {
                type = typeof(long);
            }

            public override bool Match(string part)
            {
                long result;
                if (!long.TryParse(part, out result))
                    return false;
                return true;
            }

            public override void Parse(string part, object[] parameters)
            {
                parameters[order] = long.Parse(part);
            }
            
            public override void Append(StringBuilder builder, object parameters)
            {
                var type = parameters.GetType();
                var value = type.GetProperty(pattern).GetValue(parameters);
                builder.Append((long)value);
            }
        }

        private class StringPart : Part
        {
            public StringPart()
            {
                type = typeof(string);
            }

            public override bool Match(string part)
            {
                return true;
            }

            public override void Parse(string part, object[] parameters)
            {
                parameters[order] = part;
            }
            
            public override void Append(StringBuilder builder, object parameters)
            {
                var type = parameters.GetType();
                var value = type.GetProperty(pattern).GetValue(parameters);
                builder.Append((string)value);
            }
        }

        private List<Part> parts = new List<Part>();
        private MethodInfo method;
        private ParameterInfo body;
        private Dictionary<string, Part> query;
        protected readonly IWaConfig config;
        private int numberOfMandatoryParts;

        
        public override string ToString()
        {
            return string.Join("/", parts.Select(p => p.pattern).ToArray());
        }

        public AbstractRoute(string pattern, MethodInfo methodInfo, IWaConfig config)
        {
            this.config = config;
            method = methodInfo;

            foreach (var part in pattern.Split(new[] { '/' }))
            {
                if (part.IndexOf('{') == 0)
                {
                    if (part[part.Length - 1] != '}')
                        throw new Exception("part must end with }");
                    var pair = part.Substring(1, part.Length - 2).Split(new[] { ':' });
                    Part newPart;
                    if (pair.Length == 2)
                    {
                        switch (pair[1])
                        {
                            case "int":
                                newPart = new IntPart();
                                break;
                            case "long":
                                newPart = new LongPart();
                                break;
                            default:
                                throw new Exception("unsupported type");
                        }
                    }
                    else
                   { 
                        newPart = new StringPart();
                    }
                    newPart.pattern = pair[0];
                    if (newPart.pattern[newPart.pattern.Length -1 ] == '?')
                    {
                        newPart.pattern = newPart.pattern.Substring(0, newPart.pattern.Length - 1);
                        newPart.optional = true;
                    }
                    var parameter = methodInfo.GetParameters().Where(p => p.Name == newPart.pattern && p.ParameterType == newPart.type).Single();
                    newPart.order = parameter.Position;
                    parts.Add(newPart);
                }
                else if (part.Length > 0)
                {
                    parts.Add(new Part { pattern = part });
                }
            }

            body = method.GetParameters().Where(p => p.GetCustomAttribute<FromBodyAttribute>() != null).SingleOrDefault();

            foreach (var parameter in method.GetParameters().Where(p => p.GetCustomAttribute<FromUriAttribute>() != null))
            {
                if (query == null)
                    query = new Dictionary<string, Part>();
                if (parameter.ParameterType == typeof(int))
                    query[parameter.Name] = new IntPart { order = parameter.Position };
                else if (parameter.ParameterType == typeof(long))
                    query[parameter.Name] = new LongPart { order = parameter.Position };
                else if (parameter.ParameterType == typeof(string))
                    query[parameter.Name] = new StringPart { order = parameter.Position };
                else if (parameter.ParameterType == typeof(bool))
                    query[parameter.Name] = new BoolPart { order = parameter.Position };
                else
                    throw new Exception("Parameter type " + parameter.ParameterType + " unsupported");

                if (parameter.HasDefaultValue)
                {
                    query[parameter.Name].optional = true;
                    query[parameter.Name].defaultValue = parameter.DefaultValue;
                }
            }

            var unmappedParameter = method.GetParameters().FirstOrDefault(p => p.GetCustomAttribute<FromBodyAttribute>() == null && p.GetCustomAttribute<FromUriAttribute>() == null
                && !parts.Any(x => x.order == p.Position));
            if (unmappedParameter != null)
                throw new Exception("Parameter " + unmappedParameter.Name + " has not been mapped in " + method.Name + " from " + method.DeclaringType.Name);

            numberOfMandatoryParts = parts.Count(p => !p.optional);
        }

        public bool Match(string[] path)
        {
            if (path.Length > parts.Count || path.Length < numberOfMandatoryParts)
                return false;

            for (var i = 0; i < path.Length; i++)
            {
                var pathPart = path[i];
                var toMatch = parts[i];
                if (!toMatch.Match(pathPart))
                    return false;
            }

            return true;
        }

        public abstract Task Invoke(string[] path, ICurrentContext context);

        protected int NumberOfParameters
        {
            get
            {
                return method.GetParameters().Length;
            }
        }

        protected void FillPathParameters(string[] path, object[] parameters)
        {
            for (var i = 0; i < path.Length; i++)
            {
                var pathPart = path[i];
                var toMatch = parts[i];
                toMatch.Parse(pathPart, parameters);
            }
        }

        protected bool HasBody
        {
            get
            {
                return body != null;
            }
        }

        protected void FillJsonBody(ICurrentContext context, object[] parameters)
        {
            var contentLength = int.Parse(context.Request.Headers["content-length"]);
            var bodyContent = new byte[contentLength];
            context.Request.Body.Read(bodyContent, 0, contentLength);
            var text = Encoding.UTF8.GetString(bodyContent);
            try
            {
                object result = JsonConvert.DeserializeObject(text, body.ParameterType);
                context.Model = result;
                parameters[body.Position] = result;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected void FillQueryParameters(ICurrentContext context, object[] parameters)
        {
            if (query != null)
            {
                foreach (var parameter in query)
                {
                    var value = context.Request.Query[parameter.Key];
                    if (value != null)
                        parameter.Value.Parse(value, parameters);
                    else if (parameter.Value.optional)
                        parameter.Value.SetDefault(parameters);
                }
            }
        }

        protected object Invoke(ICurrentContext context, object[] parameters)
        {
            var obj = config.Container.GetInstance(method.DeclaringType);
            return method.Invoke(obj, parameters);
        }

        internal string CreateLink(object parameters)
        {
            var ret = new StringBuilder();
            foreach (var part in parts)
            {
                ret.Append("/");
                part.Append(ret, parameters);
            }
            return ret.ToString();
        }
    }
}
