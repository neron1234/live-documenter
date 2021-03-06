﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using TheBoxSoftware.API.LiveDocumenter;

namespace Test.API.LiveDocumenter.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            Documentation docs = LiveDocumenter.Models.Docs.Get();
            TableOfContents document = docs.GetTableOfContents();
            return this.Content(string.Empty, "text/xml");
        }
    }
}
