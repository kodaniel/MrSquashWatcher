using System.ComponentModel;

namespace MrSquash.Application;

public enum AppThemes
{
    [Description("Rendszer")]
    System = 0,
    [Description("Világos")]
    Light = 1,
    [Description("Sötét")]
    Dark = 2
}