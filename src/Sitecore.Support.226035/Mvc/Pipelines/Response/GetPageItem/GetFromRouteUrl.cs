// Sitecore.Mvc.Pipelines.Response.GetPageItem.GetFromRouteUrl
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Mvc.Configuration;
using Sitecore.Mvc.Data;
using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Pipelines.Response.GetPageItem;
using Sitecore.Sites;
using System;
using System.Web.Routing;
namespace Sitecore.Support.Mvc.Pipelines.Response.GetPageItem
{

  public class GetFromRouteUrl : GetPageItemProcessor
  {
    public override void Process(GetPageItemArgs args)
    {
      var curItem = Sitecore.Context.Item;
      var name = curItem.Name;
      Assert.ArgumentNotNull(args, "args");
      if (args.Result == null)
      {
        args.Result = this.ResolveItem(args);
      }
    }

    protected virtual Item ResolveItem(GetPageItemArgs args)
    {
      string path = this.GetPath(args.RouteData);
      if (string.IsNullOrEmpty(path))
      {
        return null;
      }
      return this.GetItem(path, args);
    }

    protected override Item GetItem(string path, GetPageItemArgs args)
    {
      ItemLocator itemLocator = MvcSettings.ItemLocator;
      itemLocator.Language = args.Language;
      Item item = itemLocator.GetItem(path);
      if (item != null)
      {
        return item;
      }
      SiteContext site = Context.Site;
      if (site != null)
      {
        string[] strArray = new string[] { site.StartPath, site.RootPath };
        foreach (string str in strArray)
        {
          if (!str.IsEmptyOrNull())
          {
            // FIX FOR BUG 226035 - BEGIN //
            var pathInfo = (String)args.RouteData.Values["pathinfo"];
            var db = Sitecore.Context.Database;
            if (pathInfo != null && db.Aliases.Exists(pathInfo))
              return null;
            // FIX FOR BUG 226035 - END //

            string pathOrId = FileUtil.MakePath(str, path, '/');
            Item item2 = itemLocator.GetItem(pathOrId);
            if (item2 != null)
            {
              return item2;
            }
          }
        }
      }
      return null;
    }

    protected string GetPath(RouteData routeData)
    {
      Route route = routeData.Route as Route;
      if (route == null)
      {
        return null;
      }
      string url = route.Url;
      if (url.IsWhiteSpaceOrNull())
      {
        return null;
      }
      string[] parts = url.Split(new char[1]
      {
            '/'
      }, StringSplitOptions.RemoveEmptyEntries);
      return this.GetPathFromParts(parts, routeData);
    }

    private string GetPathFromParts(string[] parts, RouteData routeData)
    {
      string ignorePrefix = MvcSettings.IgnoreKeyPrefix;
      string text = string.Empty;
      foreach (string part in parts)
      {
        string text2 = this.ReplaceToken(part, routeData, (string key) => !routeData.Values.ContainsKey(ignorePrefix + key));
        if (!text2.IsEmptyOrNull())
        {
          text = text + "/" + text2;
        }
      }
      return text;
    }
  }
}