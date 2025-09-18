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
	public class CSVConverterEngineTests
	{
		private CSVConverterEngine _classUnderTest;
		private string _inputFile = @"..\..\..\Input\Addresses.csv";
		private string _inputFile2 = @"..\..\..\Input\Addresses2.csv";
		private string _inputFileCulture = @"..\..\..\Input\culturetest.csv";
		private string _inputFileQuotationMarks = @"..\..\..\Input\quotationmarks.csv";

		[TestInitialize]
		public void Setup()
		{
			_classUnderTest = new CSVConverterEngine();
		}

		private ConfigurationData GetImportConfiguration(bool hasColumnNames = true)
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					StartRowIndexCSV = 2,
					SeparatorCSVColumn = ';',
					HasColumnNamesCSV = hasColumnNames,
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
							PropertySource = hasColumnNames  ? "KundenNr" : "0"
						},
						new DataProperty
						{
							PropertyTarget = "CompanyName",
							PropertySource = hasColumnNames  ? "Unternehmen" : "1"
						},
						new DataProperty
						{
							PropertyTarget = "Street",
							PropertySource = hasColumnNames  ? "Strasse" : "2"
						},
						new DataProperty
						{
							PropertyTarget = "ZIPCode",
							PropertySource = hasColumnNames  ? "PLZ" : "3"
						},
						new DataProperty
						{
							PropertyTarget = "Place",
							PropertySource = hasColumnNames  ? "Ort" : "4"
						},
						new DataProperty
						{
							PropertyTarget = "Country",
							PropertySource = hasColumnNames  ? "Land" : "5",
							ValueMappings = new List<MappingPair>
							{
								new MappingPair { Source = "DE", Target = "Deutschland" }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
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
									PropertyTarget = "Value",
									PropertySource = hasColumnNames  ? "EmailZentrale" : "6"
								 }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
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
									PropertyTarget = "Value",
									PropertySource = hasColumnNames  ? "TelefonZentrale" : "7"
								 }
							}
						},
						new DataProperty {
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
									PropertySource = hasColumnNames  ? "Vorname" : "9"
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PropertySource = hasColumnNames  ? "Nachname" : "10"
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PropertySource = hasColumnNames  ? "Anrede" : "8"
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PropertySource = hasColumnNames  ? "Position" : "11"
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
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
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Email" : "12"
										 }
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
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
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Telefon" : "13"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
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
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Mobil" : "14"
										 }
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
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					StartRowIndexCSV = 2,
					SeparatorCSVColumn = ';',
					HasColumnNamesCSV = true,
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
							PropertySource = "KundenNr"
						},
						new DataProperty
						{
							PropertyTarget = "CompanyName",
							PropertySource = "Unternehmen"
						},
						new DataProperty
						{
							PropertyTarget = "Address",
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "Street",
									PropertySource = "Strasse"
								},
								new DataProperty
								{
									PropertyTarget = "ZIPCode",
									PropertySource = "PLZ"
								},
								new DataProperty
								{
									PropertyTarget = "Place",
									PropertySource = "Ort"
								},
								new DataProperty
								{
									PropertyTarget = "Country",
									PropertySource = "Land",
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
									PropertyTarget = "Value",
									PropertySource = "EmailZentrale"
								 }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
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
									PropertyTarget = "Value",
									PropertySource = "TelefonZentrale"
								 }
							}
						},
						new DataProperty {
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
									PropertySource = "Vorname"
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PropertySource = "Nachname"
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PropertySource = "Anrede"
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PropertySource = "Position"
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
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
											PropertyTarget = "Value",
											PropertySource = "Email"
										 }
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
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
											PropertyTarget = "Value",
											PropertySource = "Telefon"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
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
											PropertyTarget = "Value",
											PropertySource = "Mobil"
										 }
									}
								}
							}
						},
					}
				}
			};
		}

		private ConfigurationData GetExportConfiguration(bool hasColumnNames = true)
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					SeparatorCSVColumn = ';',
					HasColumnNamesCSV = hasColumnNames,
					PropertySourceIndexCSV = -1,
					DataProperties = new List<DataProperty>
					{
						new DataProperty
						{
							PropertyTarget = "AddressNumber",
							PropertySource = hasColumnNames  ? "KundenNr" : "0",
							PropertySourceIndexCSV = 0
						},
						new DataProperty
						{
							PropertyTarget = "CompanyName",
							PropertySource = hasColumnNames  ? "Unternehmen" : "1",
							PropertySourceIndexCSV = 1
						},
						new DataProperty
						{
							PropertyTarget = "Street",
							PropertySource = hasColumnNames  ? "Strasse" : "2",
							PropertySourceIndexCSV = 2
						},
						new DataProperty
						{
							PropertyTarget = "ZIPCode",
							PropertySource = hasColumnNames  ? "PLZ" : "3",
							PropertySourceIndexCSV = 3
						},
						new DataProperty
						{
							PropertyTarget = "Place",
							PropertySource = hasColumnNames  ? "Ort" : "4",
							PropertySourceIndexCSV = 4
						},
						new DataProperty
						{
							PropertyTarget = "Country",
							PropertySource = hasColumnNames  ? "Land" : "5",
							PropertySourceIndexCSV = 5,
							ValueMappings = new List<MappingPair>
							{
								new MappingPair { Source = "DE", Target = "Deutschland" }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							PropertySourceIndexCSV = -1,
							IsSameDataRecord = true,
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Value",
									PropertySource = hasColumnNames  ? "EmailZentrale" : "6",
									PropertySourceIndexCSV = 6,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "mail"
								 }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							PropertySourceIndexCSV = -1,
							IsSameDataRecord = true,
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Value",
									PropertySource = hasColumnNames  ? "TelefonZentrale" : "7",
									PropertySourceIndexCSV = 7,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "phone"
								 }
							}
						},
						new DataProperty {
							PropertyTarget = "Contacts",
							PropertySourceIndexCSV = -1,
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "FirstName",
									PropertySource = hasColumnNames  ? "Vorname" : "9",
									PropertySourceIndexCSV = 9
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PropertySource = hasColumnNames  ? "Nachname" : "10",
									PropertySourceIndexCSV = 10
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PropertySource = hasColumnNames  ? "Anrede" : "8",
									PropertySourceIndexCSV = 8
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PropertySource = hasColumnNames  ? "Position" : "11",
									PropertySourceIndexCSV = 11
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									PropertySourceIndexCSV = -1,
									IsSameDataRecord = true,
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Email" : "12",
											PropertySourceIndexCSV = 12,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mail"
										 }
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									PropertySourceIndexCSV = -1,
									IsSameDataRecord = true,
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Telefon" : "13",
											PropertySourceIndexCSV = 13,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "phone"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									PropertySourceIndexCSV = -1,
									IsSameDataRecord = true,
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Mobil" : "14",
											PropertySourceIndexCSV = 14,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mobile"
										 }
									}
								}
							}
						},
					}
				}
			};
		}

		private ConfigurationData GetExportConfigurationForCompany(bool hasColumnNames = true)
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					SeparatorCSVColumn = ';',
					HasColumnNamesCSV = hasColumnNames,
					PropertySourceIndexCSV = -1,
					DataProperties = new List<DataProperty>
					{
						new DataProperty
						{
							PropertyTarget = "AddressNumber",
							PropertySource = hasColumnNames  ? "KundenNr" : "0",
							PropertySourceIndexCSV = 0
						},
						new DataProperty
						{
							PropertyTarget = "CompanyName",
							PropertySource = hasColumnNames  ? "Unternehmen" : "1",
							PropertySourceIndexCSV = 1
						},
						new DataProperty
						{
							PropertyTarget = "Address",
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "Street",
									PropertySource = hasColumnNames  ? "Strasse" : "2",
									PropertySourceIndexCSV = 2
								},
								new DataProperty
								{
									PropertyTarget = "ZIPCode",
									PropertySource = hasColumnNames  ? "PLZ" : "3",
									PropertySourceIndexCSV = 3
								},
								new DataProperty
								{
									PropertyTarget = "Place",
									PropertySource = hasColumnNames  ? "Ort" : "4",
									PropertySourceIndexCSV = 4
								},
								new DataProperty
								{
									PropertyTarget = "Country",
									PropertySource = hasColumnNames  ? "Land" : "5",
									PropertySourceIndexCSV = 5,
									ValueMappings = new List<MappingPair>
									{
										new MappingPair { Source = "DE", Target = "Deutschland" }
									}
								},
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							PropertySourceIndexCSV = -1,
							IsSameDataRecord = true,
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Value",
									PropertySource = hasColumnNames  ? "EmailZentrale" : "6",
									PropertySourceIndexCSV = 6,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "mail"
								 }
							}
						},
						new DataProperty
						{
							PropertyTarget = "Communications",
							PropertySourceIndexCSV = -1,
							IsSameDataRecord = true,
							DataProperties = new List<DataProperty>
							{
								 new DataProperty
								 {
									PropertyTarget = "Value",
									PropertySource = hasColumnNames  ? "TelefonZentrale" : "7",
									PropertySourceIndexCSV = 7,
									ConditionalPropertyName = "Type",
									ConditionalPropertyValue = "phone"
								 }
							}
						},
						new DataProperty {
							PropertyTarget = "Contacts",
							PropertySourceIndexCSV = -1,
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "FirstName",
									PropertySource = hasColumnNames  ? "Vorname" : "9",
									PropertySourceIndexCSV = 9
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PropertySource = hasColumnNames  ? "Nachname" : "10",
									PropertySourceIndexCSV = 10
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PropertySource = hasColumnNames  ? "Anrede" : "8",
									PropertySourceIndexCSV = 8
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PropertySource = hasColumnNames  ? "Position" : "11",
									PropertySourceIndexCSV = 11
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									PropertySourceIndexCSV = -1,
									IsSameDataRecord = true,
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Email" : "12",
											PropertySourceIndexCSV = 12,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mail"
										 }
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									PropertySourceIndexCSV = -1,
									IsSameDataRecord = true,
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Telefon" : "13",
											PropertySourceIndexCSV = 13,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "phone"
										 },
									}
								},
								new DataProperty
								{
									PropertyTarget = "Communications",
									PropertySourceIndexCSV = -1,
									IsSameDataRecord = true,
									DataProperties = new List<DataProperty>
									{
										 new DataProperty
										 {
											PropertyTarget = "Value",
											PropertySource = hasColumnNames  ? "Mobil" : "14",
											PropertySourceIndexCSV = 14,
											ConditionalPropertyName = "Type",
											ConditionalPropertyValue = "mobile"
										 }
									}
								}
							}
						},
					}
				}
			};
		}

		[TestMethod]
		public void CSVImportConvertWithoutRemoveDoubletsTest()
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
			addresses[4].Country.Should().Be("DEU");
			addresses[4].Communications.First().Type.Should().Be("mail");
			addresses[4].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void CSVImportConvertWithRemoveDoubletsTest()
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
		public void CSVImportConvertWithoutRemoveDoubletsWithoutColumnHeaderRowTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile2, Encoding.UTF8);
			var configuration = GetImportConfiguration(false);

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
			addresses[4].Country.Should().Be("DEU");
			addresses[4].Communications.First().Type.Should().Be("mail");
			addresses[4].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void CSVImportConvertCompanyWithoutRemoveDoubletsTest()
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
			addresses[4].Address.Country.Should().Be("DEU");
			addresses[4].Communications.First().Type.Should().Be("mail");
			addresses[4].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void CSVExportConvertTest()
		{
			// Arrange
			var data = _classUnderTest.Convert<Address>(GetImportConfiguration().DataProperty, File.ReadAllText(_inputFile, Encoding.UTF8), true).ToList();
			var configuration = GetExportConfiguration();

			// Act
			var csvData = _classUnderTest.Convert(configuration.DataProperty, data);

			// Assert
			var rows = csvData.First().Split('\n');
			rows.Length.Should().Be(5);
			rows[0].TrimEnd('\r').Should().Be("KundenNr;Unternehmen;Strasse;PLZ;Ort;Land;EmailZentrale;TelefonZentrale;Anrede;Vorname;Nachname;Position;Email;Telefon;Mobil");
			rows[1].TrimEnd('\r').Should().Be("Kdn0001;Alphabet;ABC Allee 123;12345;Digenskirchen;DE;info@alphabet.de;+4985 123450;Herr;Holger;Wastow;F&E;h.wastow@alphabet.de;+4985 1234567;+49151 89898989");
			rows[2].TrimEnd('\r').Should().Be("Kdn0001;Alphabet;ABC Allee 123;12345;Digenskirchen;DE;info@alphabet.de;+4985 123450;Frau;Emmen;Taler;Kundenservice;e.taler@alphabet.de;+4985 1234589;");
			rows[3].TrimEnd('\r').Should().Be("Kdn0003;TWC AG;Deep Link Avenue 1;45110;Pirna;DE;;;Frau;Lasmiranda;Dennsiewillja;Sonstiges;l.d@twc-ag.net;;");
			rows[4].TrimEnd('\r').Should().Be("Kdn0002;Spielwaren GmbH;Fun Road 666;98765;Entenhausen;DEU;contact@spielwaren-gmbh.de;;;;;;;;");
		}

		[TestMethod]
		public void CSVExportConvertNoDataTest()
		{
			// Arrange
			var data = new List<Address>();
			var configuration = GetExportConfiguration();

			// Act
			var csvData = _classUnderTest.Convert(configuration.DataProperty, data);

			// Assert
			var rows = csvData.First().Split('\n');
			rows.Length.Should().Be(1);
			rows[0].TrimEnd('\r').Should().Be("KundenNr;Unternehmen;Strasse;PLZ;Ort;Land;EmailZentrale;TelefonZentrale;Anrede;Vorname;Nachname;Position;Email;Telefon;Mobil");
		}


		[TestMethod]
		public void CSVExportConvertForCompanyTest()
		{
			// Arrange
			var data = _classUnderTest.Convert<Company>(GetImportConfigurationForCompany().DataProperty, File.ReadAllText(_inputFile, Encoding.UTF8), true).ToList();
			var configuration = GetExportConfigurationForCompany();

			// Act
			var csvData = _classUnderTest.Convert(configuration.DataProperty, data);

			// Assert
			var rows = csvData.First().Split('\n');
			rows.Length.Should().Be(5);
			rows[0].TrimEnd('\r').Should().Be("KundenNr;Unternehmen;Strasse;PLZ;Ort;Land;EmailZentrale;TelefonZentrale;Anrede;Vorname;Nachname;Position;Email;Telefon;Mobil");
			rows[1].TrimEnd('\r').Should().Be("Kdn0001;Alphabet;ABC Allee 123;12345;Digenskirchen;DE;info@alphabet.de;+4985 123450;Herr;Holger;Wastow;F&E;h.wastow@alphabet.de;+4985 1234567;+49151 89898989");
			rows[2].TrimEnd('\r').Should().Be("Kdn0001;Alphabet;ABC Allee 123;12345;Digenskirchen;DE;info@alphabet.de;+4985 123450;Frau;Emmen;Taler;Kundenservice;e.taler@alphabet.de;+4985 1234589;");
			rows[3].TrimEnd('\r').Should().Be("Kdn0003;TWC AG;Deep Link Avenue 1;45110;Pirna;DE;;;Frau;Lasmiranda;Dennsiewillja;Sonstiges;l.d@twc-ag.net;;");
			rows[4].TrimEnd('\r').Should().Be("Kdn0002;Spielwaren GmbH;Fun Road 666;98765;Entenhausen;DEU;contact@spielwaren-gmbh.de;;;;;;;;");
		}

		[TestMethod]
		public void ImportGermanCultureTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFileCulture, Encoding.UTF8);
			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					SeparatorCSVColumn = ';',
					HasColumnNamesCSV = false,
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PropertySource = "0"
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PropertySource = "1"
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PropertySource = "2"
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PropertySource = "3"
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PropertySource = "4"
						},
						new DataProperty {
							PropertyTarget = "DateTimeOne",
							PropertySource = "5"
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
				NumberOfNull = null,
				DateTimeOne = new System.DateTime(2021, 9, 16, 20, 0, 0, System.DateTimeKind.Unspecified)
			};

			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					SeparatorCSVColumn = ';',
					HasColumnNamesCSV = false,
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PropertySource = "0",
							PropertySourceIndexCSV = 0
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PropertySource = "1",
							PropertySourceIndexCSV = 1
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PropertySource = "2",
							PropertySourceIndexCSV = 2
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PropertySource = "3",
							PropertySourceIndexCSV = 3
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PropertySource = "4",
							PropertySourceIndexCSV = 4
						},
						new DataProperty {
							PropertyTarget = "DateTimeOne",
							PropertySource = "5",
							PropertySourceIndexCSV = 5
						},
						new DataProperty {
							PropertyTarget = "NumberOfNull",
							PropertySource = "6",
							PropertySourceIndexCSV = 6
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.Convert(configuration.DataProperty, new List<CultureTest> { cultureTest }).ToList();

			// Assert
			var data = File.ReadAllText(_inputFileCulture, Encoding.UTF8).Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");

			result.Should().HaveCount(1);
			result.Single().Replace(" ", "").Replace("\r", "").Replace("\n", "").Should().Be(data);
		}

		[TestMethod]
		public void ImportQuotationMarksTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFileQuotationMarks, Encoding.UTF8);
			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					SeparatorCSVColumn = ',',
					HasColumnNamesCSV = true,
					HasSurroundingQuotationMarksCSV = true,
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PropertySource = "One"
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PropertySource = "Two"
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PropertySource = "Three"
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PropertySource = "Four"
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PropertySource = "Five"
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
		}

		[TestMethod]
		public void ExportQuotationMarksTest()
		{
			// Arrange
			var cultureTest = new CultureTest
			{
				NumberOne = 10.15m,
				NumberTwo = 20.25,
				NumberThree = 30.35f,
				NumberFour = 40,
				NumberFive = 50L
			};

			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Csv,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					SeparatorCSVColumn = ',',
					HasColumnNamesCSV = true,
					HasSurroundingQuotationMarksCSV = true,
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PropertySource = "One",
							PropertySourceIndexCSV = 0
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PropertySource = "Two",
							PropertySourceIndexCSV = 1
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PropertySource = "Three",
							PropertySourceIndexCSV = 2
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PropertySource = "Four",
							PropertySourceIndexCSV = 3
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PropertySource = "Five",
							PropertySourceIndexCSV = 4
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.Convert(configuration.DataProperty, new List<CultureTest> { cultureTest }).ToList();

			// Assert
			var data = File.ReadAllText(_inputFileQuotationMarks, Encoding.UTF8).Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");

			result.Should().HaveCount(1);
			result.Single().Replace(" ", "").Replace("\r", "").Replace("\n", "").Should().Be(data);
		}
	}
}