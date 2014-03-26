using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Examine.Session
{
    //TODO: I don't think this class is thread safe! - but I'm not sure it's supposed to be?

    internal class RequestScoped<TValue>
    {        
        [ThreadStatic]
        private static TValue _threadScoped;

        [ThreadStatic]
        private static bool _initialized;

        private readonly Func<TValue> _defaultValue;

        public RequestScoped(Func<TValue> defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public TValue Instance
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    TValue value;                
                    if (HttpContext.Current.Items[this] == null)
                    {
                        HttpContext.Current.Items[this] = value = _defaultValue();
                    }
                    else
                    {
                        value = (TValue) HttpContext.Current.Items[this];
                    }
                    return value;
                }

                if (!_initialized)
                {
                    _threadScoped = _defaultValue();
                    _initialized = true;
                }

                return _threadScoped;
            } 
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items[this] = value;
                }
                else
                {
                    _initialized = true;
                    _threadScoped = value;
                }
            }
        }

        public void Reset()
        {
            _initialized = false;
            _threadScoped = default(TValue);
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Remove(this);
            }
        }        
    }
}
