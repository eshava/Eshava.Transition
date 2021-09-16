using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Eshava.Test.Transition.Models;
using Eshava.Transition.Engines;
using Eshava.Transition.Enums;
using Eshava.Transition.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Transition.Engines
{
	[TestClass, TestCategory("Engines")]
	public class EdiConverterEngineTests
	{
		/* Testaufbau
         *  1Z  P000-P009 AddressNr
         *      P010-P109 Unternehmen
         *      P110-P209 Straße
         *  2Z  P000-P009 PLZ
         *      P010-P109 Ort
         *      P110-P159 Land
         *  3Z  P000-P049 EmailZentrale
         *      P050-P069 TelefonZentrale
         *      P070-P169 Homepage
         *  4Z  P000-P019 Anrede
         *      P020-P069 Vorname
         *      P070-P119 Nachname
         *      P120-P169 Position
         *  5Z  P000-P099 Email
         *      P100-P119 Telefon
         *      P120-P139 Mobile
         */

		private EDIConverterEngine _classUnderTest;
		private string _inputFile = @"..\..\..\Input\Addresses.edi";
		private string _inputFile2 = @"..\..\..\Input\Addresses2.edi";
		private string _inputFileCulture = @"..\..\..\Input\culturetest.edi";

		[TestInitialize]
		public void Setup()
		{
			_classUnderTest = new EDIConverterEngine();
		}

		private ConfigurationData GetImportConfiguration()
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Edi,
				DataProperty = new DataProperty
				{
					MappingProperties = new List<string>
					{
						"AddressNumber",
						"CompanyName"
					},
					CanRepeatEDI = true,
					DataProperties = new List<DataProperty>
					{
						new DataProperty
						{
							PropertyTarget = "AddressNumber",
							PositionEDI = 0,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "CompanyName",
							PositionEDI = 10,
							LengthEDI = 100,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "Street",
							PositionEDI = 110,
							LengthEDI = 100,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "ZIPCode",
							PositionEDI = 0,
							LengthEDI = 10,
							LineIndexEDI = 1
						},
						new DataProperty
						{
							PropertyTarget = "Place",
							PositionEDI = 10,
							LengthEDI = 100,
							LineIndexEDI = 1
						},
						new DataProperty
						{
							PropertyTarget = "Country",
							PositionEDI = 110,
							LengthEDI = 50,
							LineIndexEDI = 1,
							ValueMappings = new List<MappingPair>
							{
								new MappingPair { Source = "DE", Target = "Deutschland" }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							IsSameDataRecord = true,
							MappingProperties = new List<string>
							{
								"Value"
							},
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Type",
									HasMapping = true,
									MappedValue = "mail"
								 },
								 new DataProperty
								 {
									PropertyTarget = "Value", /* Email headquarters */
                                    PositionEDI = 0,
									LengthEDI = 50,
									LineIndexEDI = 2,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "mail"
								 },
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							IsSameDataRecord = true,
							MappingProperties = new List<string>
							{
								"Value"
							},
							DataProperties = new List<DataProperty> {
								 new DataProperty
								 {
									PropertyTarget = "Type",
									HasMapping = true,
									MappedValue = "phone"
								 },
								 new DataProperty
								 {
									PropertyTarget = "Value", /* Phone headquarters */
                                    PositionEDI = 50,
									LengthEDI = 20,
									LineIndexEDI = 2,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "phone"
								 },
							}
						},
						new DataProperty
						{
							PropertyTarget = "Contacts",
							MappingProperties = new List<string>
							{
									"FirstName",
									"LastName"
							},
							DataProperties = new List<DataProperty> {
								new DataProperty
								{
									PropertyTarget = "FirstName",
									PositionEDI = 20,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PositionEDI = 70,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PositionEDI = 0,
									LengthEDI = 20,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PositionEDI = 120,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "mail"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* email */
                                            PositionEDI = 0,
											LengthEDI = 100,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mail"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "phone"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* phone */
                                            PositionEDI = 100,
											LengthEDI = 20,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "phone"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "mobile"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* mobile */
                                            PositionEDI = 120,
											LengthEDI = 20,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mobile"
										 },
									}
								}
							}
						},
					}
				}
			};
		}

		private ConfigurationData GetImportConfigurationForCompany()
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Edi,
				DataProperty = new DataProperty
				{
					MappingProperties = new List<string>
					{
						"AddressNumber",
						"CompanyName"
					},
					CanRepeatEDI = true,
					DataProperties = new List<DataProperty>
					{
						new DataProperty
						{
							PropertyTarget = "AddressNumber",
							PositionEDI = 0,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "CompanyName",
							PositionEDI = 10,
							LengthEDI = 100,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "Address",
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Street",
									PositionEDI = 110,
									LengthEDI = 100,
									LineIndexEDI = 0
								},
								new DataProperty
								{
									PropertyTarget = "ZIPCode",
									PositionEDI = 0,
									LengthEDI = 10,
									LineIndexEDI = 1
								},
								new DataProperty
								{
									PropertyTarget = "Place",
									PositionEDI = 10,
									LengthEDI = 100,
									LineIndexEDI = 1
								},
								new DataProperty
								{
									PropertyTarget = "Country",
									PositionEDI = 110,
									LengthEDI = 50,
									LineIndexEDI = 1,
									ValueMappings = new List<MappingPair>
									{
										new MappingPair { Source = "DE", Target = "Deutschland" }
									}
								}
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							IsSameDataRecord = true,
							MappingProperties = new List<string>
							{
								"Value"
							},
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Type",
									HasMapping = true,
									MappedValue = "mail"
								 },
								 new DataProperty
								 {
									PropertyTarget = "Value", /* Email headquarters */
                                    PositionEDI = 0,
									LengthEDI = 50,
									LineIndexEDI = 2,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "mail"
								 },
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							IsSameDataRecord = true,
							MappingProperties = new List<string>
							{
								"Value"
							},
							DataProperties = new List<DataProperty> {
								 new DataProperty
								 {
									PropertyTarget = "Type",
									HasMapping = true,
									MappedValue = "phone"
								 },
								 new DataProperty
								 {
									PropertyTarget = "Value", /* Phone headquarters */
                                    PositionEDI = 50,
									LengthEDI = 20,
									LineIndexEDI = 2,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "phone"
								 }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Contacts",
							MappingProperties = new List<string>
							{
									"FirstName",
									"LastName"
							},
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "FirstName",
									PositionEDI = 20,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PositionEDI = 70,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PositionEDI = 0,
									LengthEDI = 20,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PositionEDI = 120,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "mail"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* email */
                                            PositionEDI = 0,
											LengthEDI = 100,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mail"
										 },
									}
								},
								new DataProperty {
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "phone"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* phone */
                                            PositionEDI = 100,
											LengthEDI = 20,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "phone"
										 },
									}
								},
								new DataProperty {
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "mobile"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* mobile */
                                            PositionEDI = 120,
											LengthEDI = 20,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mobile"
										 },
									}
								}
							}
						},
					}
				}
			};
		}

		private ConfigurationData GetImportConfiguration2()
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Edi,
				DataProperty = new DataProperty
				{
					MappingProperties = new List<string>
					{
						"AddressNumber",
						"CompanyName"
					},
					DataProperties = new List<DataProperty>
					{
						new DataProperty
						{
							PropertyTarget = "AddressNumber",
							PositionEDI = 0,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "CompanyName",
							PositionEDI = 10,
							LengthEDI = 100,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "Street",
							PositionEDI = 110,
							LengthEDI = 100,
							LineIndexEDI = 0
						},
						new DataProperty
						{
							PropertyTarget = "ZIPCode",
							PositionEDI = 0,
							LengthEDI = 10,
							LineIndexEDI = 1
						},
						new DataProperty
						{
							PropertyTarget = "Place",
							PositionEDI = 10,
							LengthEDI = 100,
							LineIndexEDI = 1
						},
						new DataProperty
						{
							PropertyTarget = "Country",
							PositionEDI = 110,
							LengthEDI = 50,
							LineIndexEDI = 1,
							ValueMappings = new List<MappingPair>
							{
								new MappingPair { Source = "DE", Target = "Deutschland" }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							IsSameDataRecord = true,
							MappingProperties = new List<string>
							{
								"Value"
							},
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Type",
									HasMapping = true,
									MappedValue = "mail"
								 },
								 new DataProperty
								 {
									PropertyTarget = "Value", /* Email headquarters */
                                    PositionEDI = 0,
									LengthEDI = 50,
									LineIndexEDI = 2,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "mail"
								 },
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							IsSameDataRecord = true,
							MappingProperties = new List<string>
							{
								"Value"
							},
							DataProperties = new List<DataProperty> {
								 new DataProperty
								 {
									PropertyTarget = "Type",
									HasMapping = true,
									MappedValue = "phone"
								 },
								 new DataProperty
								 {
									PropertyTarget = "Value", /* Phone headquarters */
                                    PositionEDI = 50,
									LengthEDI = 20,
									LineIndexEDI = 2,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "phone"
								 },
							}
						},
						new DataProperty
						{
							PropertyTarget = "Contacts",
							CanRepeatEDI = true,
							MappingProperties = new List<string>
							{
									"FirstName",
									"LastName"
							},
							DataProperties = new List<DataProperty> {
								new DataProperty
								{
									PropertyTarget = "FirstName",
									PositionEDI = 20,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PositionEDI = 70,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PositionEDI = 0,
									LengthEDI = 20,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PositionEDI = 120,
									LengthEDI = 50,
									LineIndexEDI = 3
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "mail"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* email */
                                            PositionEDI = 0,
											LengthEDI = 100,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mail"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "phone"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* phone */
                                            PositionEDI = 100,
											LengthEDI = 20,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "phone"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									IsSameDataRecord = true,
									MappingProperties = new List<string>
									{
										"Value"
									},
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Type",
											HasMapping = true,
											MappedValue = "mobile"
										 },
										 new DataProperty
										 {
											PropertyTarget = "Value", /* mobile */
                                            PositionEDI = 120,
											LengthEDI = 20,
											LineIndexEDI = 4,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mobile"
										 },
									}
								}
							}
						},
					}
				}
			};
		}

		[TestMethod]
		public void EdiConvertWithoutRemoveDoubletsTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfiguration();
			
			// Act
			var addresses = _classUnderTest.Convert<Address>(configuration.DataProperty, data, false).ToList();

			// Assert
			addresses.Should().HaveCount(5);
			addresses[0].Communications.Should().HaveCount(2);
			addresses[0].Contacts.Should().HaveCount(0);
			addresses[0].AddressNumber.Should().Be("Kdn0001");
			addresses[0].CompanyName.Should().Be("Alphabet");
			addresses[0].Street.Should().Be("ABC Allee 123");
			addresses[0].Place.Should().Be("Digenskirchen");
			addresses[0].ZIPCode.Should().Be(12345);
			addresses[0].Country.Should().Be("Deutschland");
			addresses[0].Communications.First().Type.Should().Be("mail");
			addresses[0].Communications.First().Value.Should().Be("info@alphabet.de");
			addresses[0].Communications.Last().Type.Should().Be("phone");
			addresses[0].Communications.Last().Value.Should().Be("+4985 123450");

			addresses[1].Communications.Should().HaveCount(0);
			addresses[1].Contacts.Should().HaveCount(1);
			addresses[1].AddressNumber.Should().Be("Kdn0003");
			addresses[1].CompanyName.Should().Be("TWC AG");
			addresses[1].Street.Should().Be("Deep Link Avenue 1");
			addresses[1].Place.Should().Be("Pirna");
			addresses[1].ZIPCode.Should().Be(45110);
			addresses[1].Country.Should().Be("Deutschland");
			addresses[1].Contacts.First().Title.Should().Be("Frau");
			addresses[1].Contacts.First().FirstName.Should().Be("Lasmiranda");
			addresses[1].Contacts.First().LastName.Should().Be("Dennsiewillja");
			addresses[1].Contacts.First().Position.Should().Be("Sonstiges");
			addresses[1].Contacts.First().Communications.Should().HaveCount(1);
			addresses[1].Contacts.First().Communications.First().Type.Should().Be("mail");
			addresses[1].Contacts.First().Communications.First().Value.Should().Be("l.d@twc-ag.net");

			addresses[2].Communications.Should().HaveCount(2);
			addresses[2].Contacts.Should().HaveCount(1);
			addresses[2].AddressNumber.Should().Be("Kdn0001");
			addresses[2].CompanyName.Should().Be("Alphabet");
			addresses[2].Street.Should().Be("ABC Allee 123");
			addresses[2].Place.Should().Be("Digenskirchen");
			addresses[2].ZIPCode.Should().Be(12345);
			addresses[2].Country.Should().Be("Deutschland");
			addresses[2].Communications.First().Type.Should().Be("mail");
			addresses[2].Communications.First().Value.Should().Be("info@alphabet.de");
			addresses[2].Communications.Last().Type.Should().Be("phone");
			addresses[2].Communications.Last().Value.Should().Be("+4985 123450");
			addresses[2].Contacts.First().Title.Should().Be("Herr");
			addresses[2].Contacts.First().FirstName.Should().Be("Holger");
			addresses[2].Contacts.First().LastName.Should().Be("Wastow");
			addresses[2].Contacts.First().Position.Should().Be("F&E");
			addresses[2].Contacts.First().Communications.Should().HaveCount(3);
			addresses[2].Contacts.First().Communications.First().Type.Should().Be("mail");
			addresses[2].Contacts.First().Communications.First().Value.Should().Be("h.wastow@alphabet.de");
			addresses[2].Contacts.First().Communications.ToList()[1].Type.Should().Be("phone");
			addresses[2].Contacts.First().Communications.ToList()[1].Value.Should().Be("+4985 1234567");
			addresses[2].Contacts.First().Communications.Last().Type.Should().Be("mobile");
			addresses[2].Contacts.First().Communications.Last().Value.Should().Be("+49151 89898989");

			addresses[3].Communications.Should().HaveCount(2);
			addresses[3].Contacts.Should().HaveCount(1);
			addresses[3].AddressNumber.Should().Be("Kdn0001");
			addresses[3].CompanyName.Should().Be("Alphabet");
			addresses[3].Street.Should().Be("ABC Allee 123");
			addresses[3].Place.Should().Be("Digenskirchen");
			addresses[3].ZIPCode.Should().Be(12345);
			addresses[3].Country.Should().Be("Deutschland");
			addresses[3].Communications.First().Type.Should().Be("mail");
			addresses[3].Communications.First().Value.Should().Be("info@alphabet.de");
			addresses[3].Communications.Last().Type.Should().Be("phone");
			addresses[3].Communications.Last().Value.Should().Be("+4985 123450");
			addresses[3].Contacts.First().Title.Should().Be("Frau");
			addresses[3].Contacts.First().FirstName.Should().Be("Emmen");
			addresses[3].Contacts.First().LastName.Should().Be("Taler");
			addresses[3].Contacts.First().Position.Should().Be("Kundenservice");
			addresses[3].Contacts.First().Communications.Should().HaveCount(2);
			addresses[3].Contacts.First().Communications.First().Type.Should().Be("mail");
			addresses[3].Contacts.First().Communications.First().Value.Should().Be("e.taler@alphabet.de");
			addresses[3].Contacts.First().Communications.Last().Type.Should().Be("phone");
			addresses[3].Contacts.First().Communications.Last().Value.Should().Be("+4985 1234589");

			addresses[4].Communications.Should().HaveCount(1);
			addresses[4].Contacts.Should().HaveCount(0);
			addresses[4].AddressNumber.Should().Be("Kdn0002");
			addresses[4].CompanyName.Should().Be("Spielwaren GmbH");
			addresses[4].Street.Should().Be("Fun Road 666");
			addresses[4].Place.Should().Be("Entenhausen");
			addresses[4].ZIPCode.Should().Be(98765);
			addresses[4].Country.Should().Be("Deutschland");
			addresses[4].Communications.First().Type.Should().Be("mail");
			addresses[4].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void EdiConvertWithRemoveDoubletsTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfiguration();
			
			// Act
			var addresses = _classUnderTest.Convert<Address>(configuration.DataProperty, data).ToList();

			// Assert
			addresses.Should().HaveCount(3);
			addresses[0].Communications.Should().HaveCount(2);
			addresses[0].Contacts.Should().HaveCount(2);

			addresses[1].Communications.Should().HaveCount(0);
			addresses[1].Contacts.Should().HaveCount(1);

			addresses[2].Communications.Should().HaveCount(1);
			addresses[2].Contacts.Should().HaveCount(0);
		}

		[TestMethod]
		public void EdiConvertWithRemoveDoubletsInput2Test()
		{
			var data = File.ReadAllText(_inputFile2, Encoding.UTF8);
			var configuration = GetImportConfiguration2();
			var addresses = _classUnderTest.Convert<Address>(configuration.DataProperty, data).ToList();

			addresses.Should().HaveCount(1);
			addresses[0].Communications.Should().HaveCount(2);
			addresses[0].Contacts.Should().HaveCount(2);
		}

		[TestMethod]
		public void EdiConvertCompanyWithoutRemoveDoubletsTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfigurationForCompany();
			
			// Act
			var addresses = _classUnderTest.Convert<Company>(configuration.DataProperty, data, false).ToList();

			// Assert
			addresses.Should().HaveCount(5);
			addresses[0].Communications.Should().HaveCount(2);
			addresses[0].Contacts.Should().HaveCount(0);
			addresses[0].AddressNumber.Should().Be("Kdn0001");
			addresses[0].CompanyName.Should().Be("Alphabet");
			addresses[0].Address.Street.Should().Be("ABC Allee 123");
			addresses[0].Address.Place.Should().Be("Digenskirchen");
			addresses[0].Address.ZIPCode.Should().Be(12345);
			addresses[0].Address.Country.Should().Be("Deutschland");
			addresses[0].Communications.First().Type.Should().Be("mail");
			addresses[0].Communications.First().Value.Should().Be("info@alphabet.de");
			addresses[0].Communications.Last().Type.Should().Be("phone");
			addresses[0].Communications.Last().Value.Should().Be("+4985 123450");

			addresses[1].Communications.Should().HaveCount(0);
			addresses[1].Contacts.Should().HaveCount(1);
			addresses[1].AddressNumber.Should().Be("Kdn0003");
			addresses[1].CompanyName.Should().Be("TWC AG");
			addresses[1].Address.Street.Should().Be("Deep Link Avenue 1");
			addresses[1].Address.Place.Should().Be("Pirna");
			addresses[1].Address.ZIPCode.Should().Be(45110);
			addresses[1].Address.Country.Should().Be("Deutschland");
			addresses[1].Contacts.First().Title.Should().Be("Frau");
			addresses[1].Contacts.First().FirstName.Should().Be("Lasmiranda");
			addresses[1].Contacts.First().LastName.Should().Be("Dennsiewillja");
			addresses[1].Contacts.First().Position.Should().Be("Sonstiges");
			addresses[1].Contacts.First().Communications.Should().HaveCount(1);
			addresses[1].Contacts.First().Communications.First().Type.Should().Be("mail");
			addresses[1].Contacts.First().Communications.First().Value.Should().Be("l.d@twc-ag.net");

			addresses[2].Communications.Should().HaveCount(2);
			addresses[2].Contacts.Should().HaveCount(1);
			addresses[2].AddressNumber.Should().Be("Kdn0001");
			addresses[2].CompanyName.Should().Be("Alphabet");
			addresses[2].Address.Street.Should().Be("ABC Allee 123");
			addresses[2].Address.Place.Should().Be("Digenskirchen");
			addresses[2].Address.ZIPCode.Should().Be(12345);
			addresses[2].Address.Country.Should().Be("Deutschland");
			addresses[2].Communications.First().Type.Should().Be("mail");
			addresses[2].Communications.First().Value.Should().Be("info@alphabet.de");
			addresses[2].Communications.Last().Type.Should().Be("phone");
			addresses[2].Communications.Last().Value.Should().Be("+4985 123450");
			addresses[2].Contacts.First().Title.Should().Be("Herr");
			addresses[2].Contacts.First().FirstName.Should().Be("Holger");
			addresses[2].Contacts.First().LastName.Should().Be("Wastow");
			addresses[2].Contacts.First().Position.Should().Be("F&E");
			addresses[2].Contacts.First().Communications.Should().HaveCount(3);
			addresses[2].Contacts.First().Communications.First().Type.Should().Be("mail");
			addresses[2].Contacts.First().Communications.First().Value.Should().Be("h.wastow@alphabet.de");
			addresses[2].Contacts.First().Communications.ToList()[1].Type.Should().Be("phone");
			addresses[2].Contacts.First().Communications.ToList()[1].Value.Should().Be("+4985 1234567");
			addresses[2].Contacts.First().Communications.Last().Type.Should().Be("mobile");
			addresses[2].Contacts.First().Communications.Last().Value.Should().Be("+49151 89898989");

			addresses[3].Communications.Should().HaveCount(2);
			addresses[3].Contacts.Should().HaveCount(1);
			addresses[3].AddressNumber.Should().Be("Kdn0001");
			addresses[3].CompanyName.Should().Be("Alphabet");
			addresses[3].Address.Street.Should().Be("ABC Allee 123");
			addresses[3].Address.Place.Should().Be("Digenskirchen");
			addresses[3].Address.ZIPCode.Should().Be(12345);
			addresses[3].Address.Country.Should().Be("Deutschland");
			addresses[3].Communications.First().Type.Should().Be("mail");
			addresses[3].Communications.First().Value.Should().Be("info@alphabet.de");
			addresses[3].Communications.Last().Type.Should().Be("phone");
			addresses[3].Communications.Last().Value.Should().Be("+4985 123450");
			addresses[3].Contacts.First().Title.Should().Be("Frau");
			addresses[3].Contacts.First().FirstName.Should().Be("Emmen");
			addresses[3].Contacts.First().LastName.Should().Be("Taler");
			addresses[3].Contacts.First().Position.Should().Be("Kundenservice");
			addresses[3].Contacts.First().Communications.Should().HaveCount(2);
			addresses[3].Contacts.First().Communications.First().Type.Should().Be("mail");
			addresses[3].Contacts.First().Communications.First().Value.Should().Be("e.taler@alphabet.de");
			addresses[3].Contacts.First().Communications.Last().Type.Should().Be("phone");
			addresses[3].Contacts.First().Communications.Last().Value.Should().Be("+4985 1234589");

			addresses[4].Communications.Should().HaveCount(1);
			addresses[4].Contacts.Should().HaveCount(0);
			addresses[4].AddressNumber.Should().Be("Kdn0002");
			addresses[4].CompanyName.Should().Be("Spielwaren GmbH");
			addresses[4].Address.Street.Should().Be("Fun Road 666");
			addresses[4].Address.Place.Should().Be("Entenhausen");
			addresses[4].Address.ZIPCode.Should().Be(98765);
			addresses[4].Address.Country.Should().Be("Deutschland");
			addresses[4].Communications.First().Type.Should().Be("mail");
			addresses[4].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void EdiExportConvertTest()
		{
			// Arrange
			var data = _classUnderTest.Convert<Address>(GetImportConfiguration().DataProperty, File.ReadAllText(_inputFile, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration();
			
			// Act
			var ediData = _classUnderTest.Convert(configuration.DataProperty, data);

			// Assert
			var rows = ediData.First().Split('\n');
			rows.Length.Should().Be(18);
			rows[0].TrimEnd('\r').Should().Be("Kdn0001   Alphabet                                                                                            ABC Allee 123                                                                                       ");
			rows[1].TrimEnd('\r').Should().Be("12345     Digenskirchen                                                                                       DE                                                                                                  ");
			rows[2].TrimEnd('\r').Should().Be("info@alphabet.de                                  +4985 123450                                                                                                                                                    ");
			rows[3].TrimEnd('\r').Should().Be("Herr                Holger                                            Wastow                                            F&E                                                                                       ");
			rows[4].TrimEnd('\r').Should().Be("h.wastow@alphabet.de                                                                                +4985 1234567       +49151 89898989                                                                           ");
			rows[5].TrimEnd('\r').Should().Be("Kdn0001   Alphabet                                                                                            ABC Allee 123                                                                                       ");
			rows[6].TrimEnd('\r').Should().Be("12345     Digenskirchen                                                                                       DE                                                                                                  ");
			rows[7].TrimEnd('\r').Should().Be("info@alphabet.de                                  +4985 123450                                                                                                                                                    ");
			rows[8].TrimEnd('\r').Should().Be("Frau                Emmen                                             Taler                                             Kundenservice                                                                             ");
			rows[9].TrimEnd('\r').Should().Be("e.taler@alphabet.de                                                                                 +4985 1234589                                                                                                 ");
			rows[10].TrimEnd('\r').Should().Be("Kdn0003   TWC AG                                                                                              Deep Link Avenue 1                                                                                  ");
			rows[11].TrimEnd('\r').Should().Be("45110     Pirna                                                                                               DE                                                                                                  ");
			rows[12].TrimEnd('\r').Should().Be("                                                                                                                                                                                                                  ");
			rows[13].TrimEnd('\r').Should().Be("Frau                Lasmiranda                                        Dennsiewillja                                     Sonstiges                                                                                 ");
			rows[14].TrimEnd('\r').Should().Be("l.d@twc-ag.net                                                                                                                                                                                                    ");
			rows[15].TrimEnd('\r').Should().Be("Kdn0002   Spielwaren GmbH                                                                                     Fun Road 666                                                                                        ");
			rows[16].TrimEnd('\r').Should().Be("98765     Entenhausen                                                                                         DE                                                                                                  ");
			rows[17].TrimEnd('\r').Should().Be("contact@spielwaren-gmbh.de                                                                                                                                                                                        ");
		}

		[TestMethod]
		public void EdiExportConvertConfiguration2Test()
		{
			// Arrange
			var data = _classUnderTest.Convert<Address>(GetImportConfiguration().DataProperty, File.ReadAllText(_inputFile, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration2();

			// Act
			var ediData = _classUnderTest.Convert(configuration.DataProperty, data).ToArray();

			// Assert
			ediData.Length.Should().Be(3);

			var rows = ediData[0].Split('\n');
			rows.Length.Should().Be(7);
			rows[0].TrimEnd('\r').Should().Be("Kdn0001   Alphabet                                                                                            ABC Allee 123                                                                                       ");
			rows[1].TrimEnd('\r').Should().Be("12345     Digenskirchen                                                                                       DE                                                                                                  ");
			rows[2].TrimEnd('\r').Should().Be("info@alphabet.de                                  +4985 123450                                                                                                                                                    ");
			rows[3].TrimEnd('\r').Should().Be("Herr                Holger                                            Wastow                                            F&E                                                                                       ");
			rows[4].TrimEnd('\r').Should().Be("h.wastow@alphabet.de                                                                                +4985 1234567       +49151 89898989                                                                           ");
			rows[5].TrimEnd('\r').Should().Be("Frau                Emmen                                             Taler                                             Kundenservice                                                                             ");
			rows[6].TrimEnd('\r').Should().Be("e.taler@alphabet.de                                                                                 +4985 1234589                                                                                                 ");


			rows = ediData[1].Split('\n');
			rows.Length.Should().Be(5);
			rows[0].TrimEnd('\r').Should().Be("Kdn0003   TWC AG                                                                                              Deep Link Avenue 1                                                                                  ");
			rows[1].TrimEnd('\r').Should().Be("45110     Pirna                                                                                               DE                                                                                                  ");
			rows[2].TrimEnd('\r').Should().Be("                                                                                                                                                                                                                  ");
			rows[3].TrimEnd('\r').Should().Be("Frau                Lasmiranda                                        Dennsiewillja                                     Sonstiges                                                                                 ");
			rows[4].TrimEnd('\r').Should().Be("l.d@twc-ag.net                                                                                                                                                                                                    ");


			rows = ediData[2].Split('\n');
			rows.Length.Should().Be(3);
			rows[0].TrimEnd('\r').Should().Be("Kdn0002   Spielwaren GmbH                                                                                     Fun Road 666                                                                                        ");
			rows[1].TrimEnd('\r').Should().Be("98765     Entenhausen                                                                                         DE                                                                                                  ");
			rows[2].TrimEnd('\r').Should().Be("contact@spielwaren-gmbh.de                                                                                                                                                                                        ");
		}

		[TestMethod]
		public void EdiExportConvertForCompanyTest()
		{
			// Arrange
			var data = _classUnderTest.Convert<Company>(GetImportConfigurationForCompany().DataProperty, File.ReadAllText(_inputFile, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfigurationForCompany();

			// Act
			var ediData = _classUnderTest.Convert(configuration.DataProperty, data);

			// Assert
			var rows = ediData.First().Split('\n');
			rows.Length.Should().Be(18);
			rows[0].TrimEnd('\r').Should().Be("Kdn0001   Alphabet                                                                                            ABC Allee 123                                                                                       ");
			rows[1].TrimEnd('\r').Should().Be("12345     Digenskirchen                                                                                       DE                                                                                                  ");
			rows[2].TrimEnd('\r').Should().Be("info@alphabet.de                                  +4985 123450                                                                                                                                                    ");
			rows[3].TrimEnd('\r').Should().Be("Herr                Holger                                            Wastow                                            F&E                                                                                       ");
			rows[4].TrimEnd('\r').Should().Be("h.wastow@alphabet.de                                                                                +4985 1234567       +49151 89898989                                                                           ");
			rows[5].TrimEnd('\r').Should().Be("Kdn0001   Alphabet                                                                                            ABC Allee 123                                                                                       ");
			rows[6].TrimEnd('\r').Should().Be("12345     Digenskirchen                                                                                       DE                                                                                                  ");
			rows[7].TrimEnd('\r').Should().Be("info@alphabet.de                                  +4985 123450                                                                                                                                                    ");
			rows[8].TrimEnd('\r').Should().Be("Frau                Emmen                                             Taler                                             Kundenservice                                                                             ");
			rows[9].TrimEnd('\r').Should().Be("e.taler@alphabet.de                                                                                 +4985 1234589                                                                                                 ");
			rows[10].TrimEnd('\r').Should().Be("Kdn0003   TWC AG                                                                                              Deep Link Avenue 1                                                                                  ");
			rows[11].TrimEnd('\r').Should().Be("45110     Pirna                                                                                               DE                                                                                                  ");
			rows[12].TrimEnd('\r').Should().Be("                                                                                                                                                                                                                  ");
			rows[13].TrimEnd('\r').Should().Be("Frau                Lasmiranda                                        Dennsiewillja                                     Sonstiges                                                                                 ");
			rows[14].TrimEnd('\r').Should().Be("l.d@twc-ag.net                                                                                                                                                                                                    ");
			rows[15].TrimEnd('\r').Should().Be("Kdn0002   Spielwaren GmbH                                                                                     Fun Road 666                                                                                        ");
			rows[16].TrimEnd('\r').Should().Be("98765     Entenhausen                                                                                         DE                                                                                                  ");
			rows[17].TrimEnd('\r').Should().Be("contact@spielwaren-gmbh.de                                                                                                                                                                                        ");
		}

		[TestMethod]
		public void ImportGermanCultureTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFileCulture, Encoding.UTF8);
			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Edi,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PositionEDI = 0,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PositionEDI = 10,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PositionEDI = 20,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PositionEDI = 30,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PositionEDI = 40,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "DateTimeOne",
							PositionEDI = 50,
							LengthEDI = 20,
							LineIndexEDI = 0
						}
					}
				}
			};

			// Act
			var cultureTest = _classUnderTest.Convert<CultureTest>(configuration.DataProperty, data, false).ToList();

			// Assert
			cultureTest.Should().HaveCount(1);
			cultureTest.Single().NumberOne.Should().Be(10.15m);
			cultureTest.Single().NumberTwo.Should().Be(20.25);
			cultureTest.Single().NumberThree.Should().Be(30.35f);
			cultureTest.Single().NumberFour.Should().Be(40);
			cultureTest.Single().NumberFive.Should().Be(50);
			cultureTest.Single().DateTimeOne.Value.Year.Should().Be(2021);
			cultureTest.Single().DateTimeOne.Value.Month.Should().Be(9);
			cultureTest.Single().DateTimeOne.Value.Day.Should().Be(16);
			cultureTest.Single().DateTimeOne.Value.Hour.Should().Be(20);
			cultureTest.Single().DateTimeOne.Value.Minute.Should().Be(0);
			cultureTest.Single().DateTimeOne.Value.Second.Should().Be(0);
			cultureTest.Single().DateTimeOne.Value.Kind.Should().Be(DateTimeKind.Unspecified);
		}

		[TestMethod]
		public void ExportGermanCultureTest()
		{
			// Arrange
			var cultureTest = new CultureTest
			{
				NumberOne = 10.15m,
				NumberTwo = 20.25,
				NumberThree = 30.35f,
				NumberFour = 40,
				NumberFive = 50L,
				DateTimeOne = new System.DateTime(2021, 9, 16, 20, 0, 0, System.DateTimeKind.Unspecified)
			};

			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Edi,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PositionEDI = 0,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PositionEDI = 10,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PositionEDI = 20,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PositionEDI = 30,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PositionEDI = 40,
							LengthEDI = 10,
							LineIndexEDI = 0
						},
						new DataProperty {
							PropertyTarget = "DateTimeOne",
							PositionEDI = 50,
							LengthEDI = 20,
							LineIndexEDI = 0
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.Convert(configuration.DataProperty, new List<CultureTest> { cultureTest }).ToList();

			// Assert
			var data = File.ReadAllText(_inputFileCulture, Encoding.UTF8);

			result.Should().HaveCount(1);
			result.Single().Should().Be(data);
		}
	}
}