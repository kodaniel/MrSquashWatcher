using Prism.Events;
using System;

namespace MrSquash.Core.Events
{
    public class ErrorEvent : PubSubEvent<Exception>
    {

    }
}
