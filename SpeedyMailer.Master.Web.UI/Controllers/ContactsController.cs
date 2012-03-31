﻿using System.Web.Mvc;
using AutoMapper;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Master.Web.UI.Controllers
{
    public class ContactsController : Controller
    {
        private readonly IContactsCSVParser contactsCSVParser;
        private readonly IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser> contactslUploadViewModelBuilder;
        private readonly IViewModelBuilder<UploadListViewModel> uploadListViewModelBuilder;
        private readonly IMappingEngine mapper;

        public ContactsController(IContactsCSVParser contactsCSVParser, IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser> contactslUploadViewModelBuilder, IViewModelBuilder<UploadListViewModel> uploadListViewModelBuilder, IMappingEngine mapper)
        {
            this.contactsCSVParser = contactsCSVParser;
            this.contactslUploadViewModelBuilder = contactslUploadViewModelBuilder;
            this.uploadListViewModelBuilder = uploadListViewModelBuilder;
            this.mapper = mapper;
        }

        //
        // GET: /List/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Upload()
        {
            var viewModel = uploadListViewModelBuilder.Build();
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Upload(UploadListModel model)
        {
            var initModel = mapper.Map<UploadListModel, InitialContactsBatchOptions>(model);
            contactsCSVParser.AddInitialContactBatchOptions(initModel);
            contactsCSVParser.ParseAndStore();
            var viewModel = contactslUploadViewModelBuilder.Build(contactsCSVParser);
            return View(viewModel);
        }

    }
}