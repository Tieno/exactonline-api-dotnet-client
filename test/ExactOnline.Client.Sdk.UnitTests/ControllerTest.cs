﻿using ExactOnline.Client.Sdk.Controllers;
using ExactOnline.Client.Sdk.Helpers;
using ExactOnline.Client.Sdk.Interfaces;
using ExactOnline.Client.Sdk.UnitTests.MockObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ExactOnline.Client.Sdk.UnitTests
{
    using ExactOnline.Client.Models.CRM;
    using ExactOnline.Client.Models.Financial;
    using ExactOnline.Client.Models.SalesInvoice;

    [TestClass()]
	public class ControllerTest
	{
		IApiConnection _mockConnection;

		[TestInitialize]
		public void Setup()
		{
			_mockConnection = new ApiConnectionMock();
		}

		[TestMethod]
		[TestCategory("Unit Test")]
		public void Controller_Constructor_With_CorrectTypeAndConnection_Succeeds()
		{
			var accountController = new Controller<Account>(_mockConnection);
		}

		[TestMethod]
		[TestCategory("Unit Test"), ExpectedException(typeof(Exception))]
		public void Controller_Constructor_WithoutValidType_Fails()
		{
			var accountController = new Controller<object>(_mockConnection);
		}

		[TestMethod]
		[TestCategory("Unit Test"), ExpectedException(typeof(ArgumentException))]
		public void Controller_Create_WithoutValidTypeAndConnection_Fails()
		{
			var accountController = new Controller<object>(null);
		}

		[TestMethod]
		[TestCategory("Unit Test")]
		public void Controller_GetIdentifierValueForCompoundKey_Fails()
		{
			// JournalStatus has a compound key
			var journalStatusController = new Controller<JournalStatusList>(_mockConnection);
			var journalStatus = new JournalStatusList();
			var exceptionThrown = false;
			var exceptionMessage = "";

			try
			{
				journalStatusController.GetIdentifierValue(journalStatus);
			}
			catch (Exception ex)
			{
				exceptionThrown = true;
				exceptionMessage = ex.Message;
			}

			Assert.IsTrue(exceptionThrown);
			Assert.AreEqual("Currently the SDK doesn't support entities with a compound key.", exceptionMessage);
		}

		[TestMethod]
		[TestCategory("Unit Test")]
		public void Controller_Delete_WithEntity_Succeeds()
		{
			var accountController = new Controller<Account>(_mockConnection);
			Account testAccount = accountController.GetEntity("dummyGUID", string.Empty);

			// Delete Entity and Test if Entity still exists
			Assert.IsTrue(accountController.Delete(testAccount));
			Assert.IsFalse(accountController.IsManagedEntity(testAccount));
		}

		[TestMethod]
		[TestCategory("Unit Test"), ExpectedException(typeof(ArgumentException))]
		public void Controller_Delete_WithoutEntity_Fails()
		{
			var accountController = new Controller<Account>(_mockConnection);
			accountController.Delete(null);
		}

		[TestMethod]
		[TestCategory("Unit Test")]
		public void Controller_Get_Succeeds()
		{
			var accountController = new Controller<Account>(_mockConnection);
			accountController.Get(string.Empty);
		}

		[TestMethod]
		[TestCategory("Unit Test")]
		public void Controller_GetMultipleTimes_Succeeds()
		{
			var accountController = new Controller<Account>(_mockConnection);

			// Get accounts again to test for double entitycontrollers
			var accounts = accountController.Get(string.Empty);
		}

		[TestMethod]
		[TestCategory("Unit Test"), ExpectedException(typeof(ArgumentException))]
		public void Controller_Update_WithoutEntity_Fails()
		{
			var accountController = new Controller<Account>(_mockConnection);
			Assert.IsFalse(accountController.Update(null));
		}

		[TestMethod]
		[TestCategory("Unit Test")]
		public void Controller_Update_WithEntity_Succeeds()
		{
			var accountController = new Controller<Account>(_mockConnection);
			Account testAccount = accountController.GetEntity("dummyGUID", string.Empty);
			Assert.IsTrue(accountController.Update(testAccount));
		}

		[TestMethod]
		[TestCategory("Unit Test")]
		public void Controller_Test_ManagedEntities_WithLinkedEntities_Succeeds()
		{
			// Test if controller registrates linked entities
			IApiConnector conn = new ApiConnectorControllerMock();
			var controllerList = new ControllerList(conn, string.Empty);

			var salesinvoicecontroller = (Controller<SalesInvoice>)controllerList.GetController<SalesInvoice>();
			var invoicelines = (Controller<SalesInvoiceLine>)controllerList.GetController<SalesInvoiceLine>();
			salesinvoicecontroller.GetManagerForEntity = controllerList.GetEntityManager;

			// Verify if sales invoice lines are registrated entities
			var invoice = salesinvoicecontroller.Get("")[0];
			SalesInvoiceLine line = ((List<SalesInvoiceLine>)invoice.SalesInvoiceLines)[0];
			Assert.IsTrue(invoicelines.IsManagedEntity(line), "SalesInvoiceLine isn't a managed entity");
		}
	}
}
