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
	public class JSONConverterEngineTests
	{
		private JSONConverterEngine _classUnderTest;
		private string _inputFile = @"..\..\..\Input\Addresses.json";
		private string _inputFile2 = @"..\..\..\Input\Addresses2.json";
		private string _inputFile3 = @"..\..\..\Input\Addresses3.json";
		private string _inputFileCulture = @"..\..\..\Input\culturetest.json";

		[TestInitialize]
		public void Setup()
		{
			_classUnderTest = new JSONConverterEngine();
		}

		private ConfigurationData GetImportConfiguration(bool wrappedInProperty = true, bool setAddressTarget = false, bool setAddressSource = true, bool splitExport = false)
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Json,
				DataProperty = new DataProperty
				{
					SplitExportResult = splitExport,
					PropertySource = wrappedInProperty ? "adressen" : null,
					MappingProperties = new List<string>
					{
						"AddressNumber",
						"CompanyName"
					},
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "AddressNumber",
							PropertySource = "kundennummer"
						},
						new DataProperty {
							PropertyTarget = "CompanyName",
							PropertySource = "unternehmen"
						},
						new DataProperty {
							PropertySource = setAddressSource ? "anschrift" : null,
							PropertyTarget = setAddressTarget ? "Address" : null,
							DataProperties= new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "Street",
									PropertySource = "strasse"
								},
								new DataProperty
								{
									PropertyTarget = "ZIPCode",
									PropertySource = "plz"
								},
								new DataProperty
								{
									PropertyTarget = "Place",
									PropertySource = "ort"
								},
								new DataProperty
								{
									PropertyTarget = "Country",
									PropertySource = "land"
								}
							},
						},
						new DataProperty {
							PropertySource = "kommunikationen",
							PropertyTarget = "Communications",
							MappingProperties = new List<string>
							{
								"Value",
								"Type"
							},
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "Type",
									PropertySource = "type",
									ValueMappings = new List<MappingPair>
									{
										new MappingPair { Source = "email", Target = "mail" },
										new MappingPair { Source = "telefon", Target = "phone" },
										new MappingPair { Source = "mobil", Target = "mobile" }
									}
								},
								new DataProperty
								{
									PropertyTarget = "Value",
									PropertySource = "wert"
								}
							},
						},
						new DataProperty {
							PropertySource = "ansprechpartner",
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
									PropertySource = "vorname"
								},
								new DataProperty
								{
									PropertyTarget = "LastName",
									PropertySource = "nachname"
								},
								new DataProperty
								{
									PropertyTarget = "Title",
									PropertySource = "anrede"
								},
								new DataProperty
								{
									PropertyTarget = "Position",
									PropertySource = "position"
								},
								new DataProperty
								{
									PropertySource = "kommunikationen",
									PropertyTarget = "Communications",
									MappingProperties = new List<string>
									{
										"Value",
										"Type"
									},
									DataProperties = new List<DataProperty>
									{
										new DataProperty
										{
											PropertyTarget = "Type",
											PropertySource = "type",
											ValueMappings = new List<MappingPair>
											{
												new MappingPair { Source = "email", Target = "mail" },
												new MappingPair { Source = "telefon", Target = "phone" },
												new MappingPair { Source = "mobil", Target = "mobile" }
											}
										},
										new DataProperty
										{
											PropertyTarget = "Value",
											PropertySource = "wert"
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
		public void JSONConvertWithoutRemoveDoubletsTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfiguration(true);

			// Act
			var addresses = _classUnderTest.Convert<Address>(configuration.DataProperty, data, false).ToList();

			// Assert
			addresses.Should().HaveCount(4);
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

			addresses[2].Communications.Should().HaveCount(0);
			addresses[2].Contacts.Should().HaveCount(2);
			addresses[2].AddressNumber.Should().Be("Kdn0001");
			addresses[2].CompanyName.Should().Be("Alphabet");
			addresses[2].Street.Should().Be("ABC Allee 123");
			addresses[2].Place.Should().Be("Digenskirchen");
			addresses[2].ZIPCode.Should().Be(12345);
			addresses[2].Country.Should().Be("Deutschland");
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
			addresses[2].Contacts.Last().Title.Should().Be("Frau");
			addresses[2].Contacts.Last().FirstName.Should().Be("Emmen");
			addresses[2].Contacts.Last().LastName.Should().Be("Taler");
			addresses[2].Contacts.Last().Position.Should().Be("Kundenservice");
			addresses[2].Contacts.Last().Communications.Should().HaveCount(2);
			addresses[2].Contacts.Last().Communications.First().Type.Should().Be("mail");
			addresses[2].Contacts.Last().Communications.First().Value.Should().Be("e.taler@alphabet.de");
			addresses[2].Contacts.Last().Communications.Last().Type.Should().Be("phone");
			addresses[2].Contacts.Last().Communications.Last().Value.Should().Be("+4985 1234589");

			addresses[3].Communications.Should().HaveCount(1);
			addresses[3].Contacts.Should().HaveCount(0);
			addresses[3].AddressNumber.Should().Be("Kdn0002");
			addresses[3].CompanyName.Should().Be("Spielwaren GmbH");
			addresses[3].Street.Should().Be("Fun Road 666");
			addresses[3].Place.Should().Be("Entenhausen");
			addresses[3].ZIPCode.Should().Be(98765);
			addresses[3].Country.Should().Be("Deutschland");
			addresses[3].Communications.First().Type.Should().Be("mail");
			addresses[3].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void JSONConvertWithRemoveDoubletsTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfiguration(true);

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
		public void JSONConvertWithRemoveDoubletsFromSource2Test()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile2, Encoding.UTF8);
			var configuration = GetImportConfiguration(false);

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
		public void JSONConvertToCompanyWithoutRemoveDoubletsTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfiguration(true, true);

			// Act
			var companies = _classUnderTest.Convert<Company>(configuration.DataProperty, data, false).ToList();

			// Assert
			companies.Should().HaveCount(4);
			companies[0].Communications.Should().HaveCount(2);
			companies[0].Contacts.Should().HaveCount(0);
			companies[0].AddressNumber.Should().Be("Kdn0001");
			companies[0].CompanyName.Should().Be("Alphabet");
			companies[0].Address.Street.Should().Be("ABC Allee 123");
			companies[0].Address.Place.Should().Be("Digenskirchen");
			companies[0].Address.ZIPCode.Should().Be(12345);
			companies[0].Address.Country.Should().Be("Deutschland");
			companies[0].Communications.First().Type.Should().Be("mail");
			companies[0].Communications.First().Value.Should().Be("info@alphabet.de");
			companies[0].Communications.Last().Type.Should().Be("phone");
			companies[0].Communications.Last().Value.Should().Be("+4985 123450");

			companies[1].Communications.Should().HaveCount(0);
			companies[1].Contacts.Should().HaveCount(1);
			companies[1].AddressNumber.Should().Be("Kdn0003");
			companies[1].CompanyName.Should().Be("TWC AG");
			companies[1].Address.Street.Should().Be("Deep Link Avenue 1");
			companies[1].Address.Place.Should().Be("Pirna");
			companies[1].Address.ZIPCode.Should().Be(45110);
			companies[1].Address.Country.Should().Be("Deutschland");
			companies[1].Contacts.First().Title.Should().Be("Frau");
			companies[1].Contacts.First().FirstName.Should().Be("Lasmiranda");
			companies[1].Contacts.First().LastName.Should().Be("Dennsiewillja");
			companies[1].Contacts.First().Position.Should().Be("Sonstiges");
			companies[1].Contacts.First().Communications.Should().HaveCount(1);
			companies[1].Contacts.First().Communications.First().Type.Should().Be("mail");
			companies[1].Contacts.First().Communications.First().Value.Should().Be("l.d@twc-ag.net");

			companies[2].Communications.Should().HaveCount(0);
			companies[2].Contacts.Should().HaveCount(2);
			companies[2].AddressNumber.Should().Be("Kdn0001");
			companies[2].CompanyName.Should().Be("Alphabet");
			companies[2].Address.Street.Should().Be("ABC Allee 123");
			companies[2].Address.Place.Should().Be("Digenskirchen");
			companies[2].Address.ZIPCode.Should().Be(12345);
			companies[2].Address.Country.Should().Be("Deutschland");
			companies[2].Contacts.First().Title.Should().Be("Herr");
			companies[2].Contacts.First().FirstName.Should().Be("Holger");
			companies[2].Contacts.First().LastName.Should().Be("Wastow");
			companies[2].Contacts.First().Position.Should().Be("F&E");
			companies[2].Contacts.First().Communications.Should().HaveCount(3);
			companies[2].Contacts.First().Communications.First().Type.Should().Be("mail");
			companies[2].Contacts.First().Communications.First().Value.Should().Be("h.wastow@alphabet.de");
			companies[2].Contacts.First().Communications.ToList()[1].Type.Should().Be("phone");
			companies[2].Contacts.First().Communications.ToList()[1].Value.Should().Be("+4985 1234567");
			companies[2].Contacts.First().Communications.Last().Type.Should().Be("mobile");
			companies[2].Contacts.First().Communications.Last().Value.Should().Be("+49151 89898989");
			companies[2].Contacts.Last().Title.Should().Be("Frau");
			companies[2].Contacts.Last().FirstName.Should().Be("Emmen");
			companies[2].Contacts.Last().LastName.Should().Be("Taler");
			companies[2].Contacts.Last().Position.Should().Be("Kundenservice");
			companies[2].Contacts.Last().Communications.Should().HaveCount(2);
			companies[2].Contacts.Last().Communications.First().Type.Should().Be("mail");
			companies[2].Contacts.Last().Communications.First().Value.Should().Be("e.taler@alphabet.de");
			companies[2].Contacts.Last().Communications.Last().Type.Should().Be("phone");
			companies[2].Contacts.Last().Communications.Last().Value.Should().Be("+4985 1234589");

			companies[3].Communications.Should().HaveCount(1);
			companies[3].Contacts.Should().HaveCount(0);
			companies[3].AddressNumber.Should().Be("Kdn0002");
			companies[3].CompanyName.Should().Be("Spielwaren GmbH");
			companies[3].Address.Street.Should().Be("Fun Road 666");
			companies[3].Address.Place.Should().Be("Entenhausen");
			companies[3].Address.ZIPCode.Should().Be(98765);
			companies[3].Address.Country.Should().Be("Deutschland");
			companies[3].Communications.First().Type.Should().Be("mail");
			companies[3].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void JSONConvertToCompanyWithoutRemoveDoubletsFromSource3Test()
		{
			// Arrange
			var data = File.ReadAllText(_inputFile3, Encoding.UTF8);
			var configuration = GetImportConfiguration(true, true, false);

			// Act
			var companies = _classUnderTest.Convert<Company>(configuration.DataProperty, data, false).ToList();

			// Assert
			companies.Should().HaveCount(4);
			companies[0].Communications.Should().HaveCount(2);
			companies[0].Contacts.Should().HaveCount(0);
			companies[0].AddressNumber.Should().Be("Kdn0001");
			companies[0].CompanyName.Should().Be("Alphabet");
			companies[0].Address.Street.Should().Be("ABC Allee 123");
			companies[0].Address.Place.Should().Be("Digenskirchen");
			companies[0].Address.ZIPCode.Should().Be(12345);
			companies[0].Address.Country.Should().Be("Deutschland");
			companies[0].Communications.First().Type.Should().Be("mail");
			companies[0].Communications.First().Value.Should().Be("info@alphabet.de");
			companies[0].Communications.Last().Type.Should().Be("phone");
			companies[0].Communications.Last().Value.Should().Be("+4985 123450");

			companies[1].Communications.Should().HaveCount(0);
			companies[1].Contacts.Should().HaveCount(1);
			companies[1].AddressNumber.Should().Be("Kdn0003");
			companies[1].CompanyName.Should().Be("TWC AG");
			companies[1].Address.Street.Should().Be("Deep Link Avenue 1");
			companies[1].Address.Place.Should().Be("Pirna");
			companies[1].Address.ZIPCode.Should().Be(45110);
			companies[1].Address.Country.Should().Be("Deutschland");
			companies[1].Contacts.First().Title.Should().Be("Frau");
			companies[1].Contacts.First().FirstName.Should().Be("Lasmiranda");
			companies[1].Contacts.First().LastName.Should().Be("Dennsiewillja");
			companies[1].Contacts.First().Position.Should().Be("Sonstiges");
			companies[1].Contacts.First().Communications.Should().HaveCount(1);
			companies[1].Contacts.First().Communications.First().Type.Should().Be("mail");
			companies[1].Contacts.First().Communications.First().Value.Should().Be("l.d@twc-ag.net");

			companies[2].Communications.Should().HaveCount(0);
			companies[2].Contacts.Should().HaveCount(2);
			companies[2].AddressNumber.Should().Be("Kdn0001");
			companies[2].CompanyName.Should().Be("Alphabet");
			companies[2].Address.Street.Should().Be("ABC Allee 123");
			companies[2].Address.Place.Should().Be("Digenskirchen");
			companies[2].Address.ZIPCode.Should().Be(12345);
			companies[2].Address.Country.Should().Be("Deutschland");
			companies[2].Contacts.First().Title.Should().Be("Herr");
			companies[2].Contacts.First().FirstName.Should().Be("Holger");
			companies[2].Contacts.First().LastName.Should().Be("Wastow");
			companies[2].Contacts.First().Position.Should().Be("F&E");
			companies[2].Contacts.First().Communications.Should().HaveCount(3);
			companies[2].Contacts.First().Communications.First().Type.Should().Be("mail");
			companies[2].Contacts.First().Communications.First().Value.Should().Be("h.wastow@alphabet.de");
			companies[2].Contacts.First().Communications.ToList()[1].Type.Should().Be("phone");
			companies[2].Contacts.First().Communications.ToList()[1].Value.Should().Be("+4985 1234567");
			companies[2].Contacts.First().Communications.Last().Type.Should().Be("mobile");
			companies[2].Contacts.First().Communications.Last().Value.Should().Be("+49151 89898989");
			companies[2].Contacts.Last().Title.Should().Be("Frau");
			companies[2].Contacts.Last().FirstName.Should().Be("Emmen");
			companies[2].Contacts.Last().LastName.Should().Be("Taler");
			companies[2].Contacts.Last().Position.Should().Be("Kundenservice");
			companies[2].Contacts.Last().Communications.Should().HaveCount(2);
			companies[2].Contacts.Last().Communications.First().Type.Should().Be("mail");
			companies[2].Contacts.Last().Communications.First().Value.Should().Be("e.taler@alphabet.de");
			companies[2].Contacts.Last().Communications.Last().Type.Should().Be("phone");
			companies[2].Contacts.Last().Communications.Last().Value.Should().Be("+4985 1234589");

			companies[3].Communications.Should().HaveCount(1);
			companies[3].Contacts.Should().HaveCount(0);
			companies[3].AddressNumber.Should().Be("Kdn0002");
			companies[3].CompanyName.Should().Be("Spielwaren GmbH");
			companies[3].Address.Street.Should().Be("Fun Road 666");
			companies[3].Address.Place.Should().Be("Entenhausen");
			companies[3].Address.ZIPCode.Should().Be(98765);
			companies[3].Address.Country.Should().Be("Deutschland");
			companies[3].Communications.First().Type.Should().Be("mail");
			companies[3].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}


		[TestMethod]
		public void JSONExportConvertTest()
		{
			// Arrange
			var data = _classUnderTest.Convert<Address>(GetImportConfiguration().DataProperty, File.ReadAllText(_inputFile, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration();

			// Act
			var jsonData = _classUnderTest.Convert(configuration.DataProperty, data).ToArray();

			// Assert
			jsonData.Length.Should().Be(1);

			var expectedResult = new StringBuilder();
			expectedResult.AppendLine("{");
			expectedResult.AppendLine("  \"adressen\": [");
			expectedResult.AppendLine("    {");
			expectedResult.AppendLine("      \"kundennummer\": \"Kdn0001\",");
			expectedResult.AppendLine("      \"unternehmen\": \"Alphabet\",");
			expectedResult.AppendLine("      \"anschrift\": {");
			expectedResult.AppendLine("        \"strasse\": \"ABC Allee 123\",");
			expectedResult.AppendLine("        \"plz\": 12345,");
			expectedResult.AppendLine("        \"ort\": \"Digenskirchen\",");
			expectedResult.AppendLine("        \"land\": \"Deutschland\"");
			expectedResult.AppendLine("      },");
			expectedResult.AppendLine("      \"kommunikationen\": [");
			expectedResult.AppendLine("        {");
			expectedResult.AppendLine("          \"type\": \"email\",");
			expectedResult.AppendLine("          \"wert\": \"info@alphabet.de\"");
			expectedResult.AppendLine("        },");
			expectedResult.AppendLine("        {");
			expectedResult.AppendLine("          \"type\": \"telefon\",");
			expectedResult.AppendLine("          \"wert\": \"+4985 123450\"");
			expectedResult.AppendLine("        }");
			expectedResult.AppendLine("      ],");
			expectedResult.AppendLine("      \"ansprechpartner\": [");
			expectedResult.AppendLine("        {");
			expectedResult.AppendLine("          \"vorname\": \"Holger\",");
			expectedResult.AppendLine("          \"nachname\": \"Wastow\",");
			expectedResult.AppendLine("          \"anrede\": \"Herr\",");
			expectedResult.AppendLine("          \"position\": \"F&E\",");
			expectedResult.AppendLine("          \"kommunikationen\": [");
			expectedResult.AppendLine("            {");
			expectedResult.AppendLine("              \"type\": \"email\",");
			expectedResult.AppendLine("              \"wert\": \"h.wastow@alphabet.de\"");
			expectedResult.AppendLine("            },");
			expectedResult.AppendLine("            {");
			expectedResult.AppendLine("              \"type\": \"telefon\",");
			expectedResult.AppendLine("              \"wert\": \"+4985 1234567\"");
			expectedResult.AppendLine("            },");
			expectedResult.AppendLine("            {");
			expectedResult.AppendLine("              \"type\": \"mobil\",");
			expectedResult.AppendLine("              \"wert\": \"+49151 89898989\"");
			expectedResult.AppendLine("            }");
			expectedResult.AppendLine("          ]");
			expectedResult.AppendLine("        },");
			expectedResult.AppendLine("        {");
			expectedResult.AppendLine("          \"vorname\": \"Emmen\",");
			expectedResult.AppendLine("          \"nachname\": \"Taler\",");
			expectedResult.AppendLine("          \"anrede\": \"Frau\",");
			expectedResult.AppendLine("          \"position\": \"Kundenservice\",");
			expectedResult.AppendLine("          \"kommunikationen\": [");
			expectedResult.AppendLine("            {");
			expectedResult.AppendLine("              \"type\": \"email\",");
			expectedResult.AppendLine("              \"wert\": \"e.taler@alphabet.de\"");
			expectedResult.AppendLine("            },");
			expectedResult.AppendLine("            {");
			expectedResult.AppendLine("              \"type\": \"telefon\",");
			expectedResult.AppendLine("              \"wert\": \"+4985 1234589\"");
			expectedResult.AppendLine("            }");
			expectedResult.AppendLine("          ]");
			expectedResult.AppendLine("        }");
			expectedResult.AppendLine("      ]");
			expectedResult.AppendLine("    },");
			expectedResult.AppendLine("    {");
			expectedResult.AppendLine("      \"kundennummer\": \"Kdn0003\",");
			expectedResult.AppendLine("      \"unternehmen\": \"TWC AG\",");
			expectedResult.AppendLine("      \"anschrift\": {");
			expectedResult.AppendLine("        \"strasse\": \"Deep Link Avenue 1\",");
			expectedResult.AppendLine("        \"plz\": 45110,");
			expectedResult.AppendLine("        \"ort\": \"Pirna\",");
			expectedResult.AppendLine("        \"land\": \"Deutschland\"");
			expectedResult.AppendLine("      },");
			expectedResult.AppendLine("      \"ansprechpartner\": [");
			expectedResult.AppendLine("        {");
			expectedResult.AppendLine("          \"vorname\": \"Lasmiranda\",");
			expectedResult.AppendLine("          \"nachname\": \"Dennsiewillja\",");
			expectedResult.AppendLine("          \"anrede\": \"Frau\",");
			expectedResult.AppendLine("          \"position\": \"Sonstiges\",");
			expectedResult.AppendLine("          \"kommunikationen\": [");
			expectedResult.AppendLine("            {");
			expectedResult.AppendLine("              \"type\": \"email\",");
			expectedResult.AppendLine("              \"wert\": \"l.d@twc-ag.net\"");
			expectedResult.AppendLine("            }");
			expectedResult.AppendLine("          ]");
			expectedResult.AppendLine("        }");
			expectedResult.AppendLine("      ]");
			expectedResult.AppendLine("    },");
			expectedResult.AppendLine("    {");
			expectedResult.AppendLine("      \"kundennummer\": \"Kdn0002\",");
			expectedResult.AppendLine("      \"unternehmen\": \"Spielwaren GmbH\",");
			expectedResult.AppendLine("      \"anschrift\": {");
			expectedResult.AppendLine("        \"strasse\": \"Fun Road 666\",");
			expectedResult.AppendLine("        \"plz\": 98765,");
			expectedResult.AppendLine("        \"ort\": \"Entenhausen\",");
			expectedResult.AppendLine("        \"land\": \"Deutschland\"");
			expectedResult.AppendLine("      },");
			expectedResult.AppendLine("      \"kommunikationen\": [");
			expectedResult.AppendLine("        {");
			expectedResult.AppendLine("          \"type\": \"email\",");
			expectedResult.AppendLine("          \"wert\": \"contact@spielwaren-gmbh.de\"");
			expectedResult.AppendLine("        }");
			expectedResult.AppendLine("      ]");
			expectedResult.AppendLine("    }");
			expectedResult.AppendLine("  ]");
			expectedResult.Append("}");

			jsonData[0].Should().Be(expectedResult.ToString());
		}

		[TestMethod]
		public void JSONExportConvertForCompanyTest()
		{
			// Arrange
			var data = _classUnderTest.Convert<Company>(GetImportConfiguration(true, true, false).DataProperty, File.ReadAllText(_inputFile3, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration(false, true, false);

			// Act
			var jsonData = _classUnderTest.Convert(configuration.DataProperty, data).ToArray();

			// Assert
			jsonData.Length.Should().Be(1);

			var expectedResult = new StringBuilder();
			expectedResult.AppendLine("[");
			expectedResult.AppendLine("  {");
			expectedResult.AppendLine("    \"kundennummer\": \"Kdn0001\",");
			expectedResult.AppendLine("    \"unternehmen\": \"Alphabet\",");
			expectedResult.AppendLine("    \"strasse\": \"ABC Allee 123\",");
			expectedResult.AppendLine("    \"plz\": 12345,");
			expectedResult.AppendLine("    \"ort\": \"Digenskirchen\",");
			expectedResult.AppendLine("    \"land\": \"Deutschland\",");
			expectedResult.AppendLine("    \"kommunikationen\": [");
			expectedResult.AppendLine("      {");
			expectedResult.AppendLine("        \"type\": \"email\",");
			expectedResult.AppendLine("        \"wert\": \"info@alphabet.de\"");
			expectedResult.AppendLine("      },");
			expectedResult.AppendLine("      {");
			expectedResult.AppendLine("        \"type\": \"telefon\",");
			expectedResult.AppendLine("        \"wert\": \"+4985 123450\"");
			expectedResult.AppendLine("      }");
			expectedResult.AppendLine("    ],");
			expectedResult.AppendLine("    \"ansprechpartner\": [");
			expectedResult.AppendLine("      {");
			expectedResult.AppendLine("        \"vorname\": \"Holger\",");
			expectedResult.AppendLine("        \"nachname\": \"Wastow\",");
			expectedResult.AppendLine("        \"anrede\": \"Herr\",");
			expectedResult.AppendLine("        \"position\": \"F&E\",");
			expectedResult.AppendLine("        \"kommunikationen\": [");
			expectedResult.AppendLine("          {");
			expectedResult.AppendLine("            \"type\": \"email\",");
			expectedResult.AppendLine("            \"wert\": \"h.wastow@alphabet.de\"");
			expectedResult.AppendLine("          },");
			expectedResult.AppendLine("          {");
			expectedResult.AppendLine("            \"type\": \"telefon\",");
			expectedResult.AppendLine("            \"wert\": \"+4985 1234567\"");
			expectedResult.AppendLine("          },");
			expectedResult.AppendLine("          {");
			expectedResult.AppendLine("            \"type\": \"mobil\",");
			expectedResult.AppendLine("            \"wert\": \"+49151 89898989\"");
			expectedResult.AppendLine("          }");
			expectedResult.AppendLine("        ]");
			expectedResult.AppendLine("      },");
			expectedResult.AppendLine("      {");
			expectedResult.AppendLine("        \"vorname\": \"Emmen\",");
			expectedResult.AppendLine("        \"nachname\": \"Taler\",");
			expectedResult.AppendLine("        \"anrede\": \"Frau\",");
			expectedResult.AppendLine("        \"position\": \"Kundenservice\",");
			expectedResult.AppendLine("        \"kommunikationen\": [");
			expectedResult.AppendLine("          {");
			expectedResult.AppendLine("            \"type\": \"email\",");
			expectedResult.AppendLine("            \"wert\": \"e.taler@alphabet.de\"");
			expectedResult.AppendLine("          },");
			expectedResult.AppendLine("          {");
			expectedResult.AppendLine("            \"type\": \"telefon\",");
			expectedResult.AppendLine("            \"wert\": \"+4985 1234589\"");
			expectedResult.AppendLine("          }");
			expectedResult.AppendLine("        ]");
			expectedResult.AppendLine("      }");
			expectedResult.AppendLine("    ]");
			expectedResult.AppendLine("  },");
			expectedResult.AppendLine("  {");
			expectedResult.AppendLine("    \"kundennummer\": \"Kdn0003\",");
			expectedResult.AppendLine("    \"unternehmen\": \"TWC AG\",");
			expectedResult.AppendLine("    \"strasse\": \"Deep Link Avenue 1\",");
			expectedResult.AppendLine("    \"plz\": 45110,");
			expectedResult.AppendLine("    \"ort\": \"Pirna\",");
			expectedResult.AppendLine("    \"land\": \"Deutschland\",");
			expectedResult.AppendLine("    \"ansprechpartner\": [");
			expectedResult.AppendLine("      {");
			expectedResult.AppendLine("        \"vorname\": \"Lasmiranda\",");
			expectedResult.AppendLine("        \"nachname\": \"Dennsiewillja\",");
			expectedResult.AppendLine("        \"anrede\": \"Frau\",");
			expectedResult.AppendLine("        \"position\": \"Sonstiges\",");
			expectedResult.AppendLine("        \"kommunikationen\": [");
			expectedResult.AppendLine("          {");
			expectedResult.AppendLine("            \"type\": \"email\",");
			expectedResult.AppendLine("            \"wert\": \"l.d@twc-ag.net\"");
			expectedResult.AppendLine("          }");
			expectedResult.AppendLine("        ]");
			expectedResult.AppendLine("      }");
			expectedResult.AppendLine("    ]");
			expectedResult.AppendLine("  },");
			expectedResult.AppendLine("  {");
			expectedResult.AppendLine("    \"kundennummer\": \"Kdn0002\",");
			expectedResult.AppendLine("    \"unternehmen\": \"Spielwaren GmbH\",");
			expectedResult.AppendLine("    \"strasse\": \"Fun Road 666\",");
			expectedResult.AppendLine("    \"plz\": 98765,");
			expectedResult.AppendLine("    \"ort\": \"Entenhausen\",");
			expectedResult.AppendLine("    \"land\": \"Deutschland\",");
			expectedResult.AppendLine("    \"kommunikationen\": [");
			expectedResult.AppendLine("      {");
			expectedResult.AppendLine("        \"type\": \"email\",");
			expectedResult.AppendLine("        \"wert\": \"contact@spielwaren-gmbh.de\"");
			expectedResult.AppendLine("      }");
			expectedResult.AppendLine("    ]");
			expectedResult.AppendLine("  }");
			expectedResult.Append("]");

			jsonData[0].Should().Be(expectedResult.ToString());
		}

		[TestMethod]
		public void JSONExportConvertForCompanySplitExportTest()
		{
			// Arrange
			var data = _classUnderTest.Convert<Company>(GetImportConfiguration(true, true, false).DataProperty, File.ReadAllText(_inputFile3, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration(false, true, false, true);

			// Act
			var jsonData = _classUnderTest.Convert(configuration.DataProperty, data).ToArray();

			// Assert
			jsonData.Length.Should().Be(3);

			var expectedResultOne = new StringBuilder();
			expectedResultOne.AppendLine("{");
			expectedResultOne.AppendLine("  \"kundennummer\": \"Kdn0001\",");
			expectedResultOne.AppendLine("  \"unternehmen\": \"Alphabet\",");
			expectedResultOne.AppendLine("  \"strasse\": \"ABC Allee 123\",");
			expectedResultOne.AppendLine("  \"plz\": 12345,");
			expectedResultOne.AppendLine("  \"ort\": \"Digenskirchen\",");
			expectedResultOne.AppendLine("  \"land\": \"Deutschland\",");
			expectedResultOne.AppendLine("  \"kommunikationen\": [");
			expectedResultOne.AppendLine("    {");
			expectedResultOne.AppendLine("      \"type\": \"email\",");
			expectedResultOne.AppendLine("      \"wert\": \"info@alphabet.de\"");
			expectedResultOne.AppendLine("    },");
			expectedResultOne.AppendLine("    {");
			expectedResultOne.AppendLine("      \"type\": \"telefon\",");
			expectedResultOne.AppendLine("      \"wert\": \"+4985 123450\"");
			expectedResultOne.AppendLine("    }");
			expectedResultOne.AppendLine("  ],");
			expectedResultOne.AppendLine("  \"ansprechpartner\": [");
			expectedResultOne.AppendLine("    {");
			expectedResultOne.AppendLine("      \"vorname\": \"Holger\",");
			expectedResultOne.AppendLine("      \"nachname\": \"Wastow\",");
			expectedResultOne.AppendLine("      \"anrede\": \"Herr\",");
			expectedResultOne.AppendLine("      \"position\": \"F&E\",");
			expectedResultOne.AppendLine("      \"kommunikationen\": [");
			expectedResultOne.AppendLine("        {");
			expectedResultOne.AppendLine("          \"type\": \"email\",");
			expectedResultOne.AppendLine("          \"wert\": \"h.wastow@alphabet.de\"");
			expectedResultOne.AppendLine("        },");
			expectedResultOne.AppendLine("        {");
			expectedResultOne.AppendLine("          \"type\": \"telefon\",");
			expectedResultOne.AppendLine("          \"wert\": \"+4985 1234567\"");
			expectedResultOne.AppendLine("        },");
			expectedResultOne.AppendLine("        {");
			expectedResultOne.AppendLine("          \"type\": \"mobil\",");
			expectedResultOne.AppendLine("          \"wert\": \"+49151 89898989\"");
			expectedResultOne.AppendLine("        }");
			expectedResultOne.AppendLine("      ]");
			expectedResultOne.AppendLine("    },");
			expectedResultOne.AppendLine("    {");
			expectedResultOne.AppendLine("      \"vorname\": \"Emmen\",");
			expectedResultOne.AppendLine("      \"nachname\": \"Taler\",");
			expectedResultOne.AppendLine("      \"anrede\": \"Frau\",");
			expectedResultOne.AppendLine("      \"position\": \"Kundenservice\",");
			expectedResultOne.AppendLine("      \"kommunikationen\": [");
			expectedResultOne.AppendLine("        {");
			expectedResultOne.AppendLine("          \"type\": \"email\",");
			expectedResultOne.AppendLine("          \"wert\": \"e.taler@alphabet.de\"");
			expectedResultOne.AppendLine("        },");
			expectedResultOne.AppendLine("        {");
			expectedResultOne.AppendLine("          \"type\": \"telefon\",");
			expectedResultOne.AppendLine("          \"wert\": \"+4985 1234589\"");
			expectedResultOne.AppendLine("        }");
			expectedResultOne.AppendLine("      ]");
			expectedResultOne.AppendLine("    }");
			expectedResultOne.AppendLine("  ]");
			expectedResultOne.Append("}");

			var expectedResultTwo = new StringBuilder();
			expectedResultTwo.AppendLine("{");
			expectedResultTwo.AppendLine("  \"kundennummer\": \"Kdn0003\",");
			expectedResultTwo.AppendLine("  \"unternehmen\": \"TWC AG\",");
			expectedResultTwo.AppendLine("  \"strasse\": \"Deep Link Avenue 1\",");
			expectedResultTwo.AppendLine("  \"plz\": 45110,");
			expectedResultTwo.AppendLine("  \"ort\": \"Pirna\",");
			expectedResultTwo.AppendLine("  \"land\": \"Deutschland\",");
			expectedResultTwo.AppendLine("  \"ansprechpartner\": [");
			expectedResultTwo.AppendLine("    {");
			expectedResultTwo.AppendLine("      \"vorname\": \"Lasmiranda\",");
			expectedResultTwo.AppendLine("      \"nachname\": \"Dennsiewillja\",");
			expectedResultTwo.AppendLine("      \"anrede\": \"Frau\",");
			expectedResultTwo.AppendLine("      \"position\": \"Sonstiges\",");
			expectedResultTwo.AppendLine("      \"kommunikationen\": [");
			expectedResultTwo.AppendLine("        {");
			expectedResultTwo.AppendLine("          \"type\": \"email\",");
			expectedResultTwo.AppendLine("          \"wert\": \"l.d@twc-ag.net\"");
			expectedResultTwo.AppendLine("        }");
			expectedResultTwo.AppendLine("      ]");
			expectedResultTwo.AppendLine("    }");
			expectedResultTwo.AppendLine("  ]");
			expectedResultTwo.Append("}");


			var expectedResultThree = new StringBuilder();
			expectedResultThree.AppendLine("{");
			expectedResultThree.AppendLine("  \"kundennummer\": \"Kdn0002\",");
			expectedResultThree.AppendLine("  \"unternehmen\": \"Spielwaren GmbH\",");
			expectedResultThree.AppendLine("  \"strasse\": \"Fun Road 666\",");
			expectedResultThree.AppendLine("  \"plz\": 98765,");
			expectedResultThree.AppendLine("  \"ort\": \"Entenhausen\",");
			expectedResultThree.AppendLine("  \"land\": \"Deutschland\",");
			expectedResultThree.AppendLine("  \"kommunikationen\": [");
			expectedResultThree.AppendLine("    {");
			expectedResultThree.AppendLine("      \"type\": \"email\",");
			expectedResultThree.AppendLine("      \"wert\": \"contact@spielwaren-gmbh.de\"");
			expectedResultThree.AppendLine("    }");
			expectedResultThree.AppendLine("  ]");
			expectedResultThree.Append("}");

			jsonData[0].Should().Be(expectedResultOne.ToString());
			jsonData[1].Should().Be(expectedResultTwo.ToString());
			jsonData[2].Should().Be(expectedResultThree.ToString());
		}

		[TestMethod]
		public void ImportGermanCultureTest()
		{
			// Arrange
			var data = File.ReadAllText(_inputFileCulture, Encoding.UTF8);
			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Json,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					SplitExportResult = false,
					PropertySource = null,
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PropertySource = "numberOne"
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PropertySource = "numberTwo"
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PropertySource = "numberThree"
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PropertySource = "numberFour"
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PropertySource = "numberFive"
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
		public void ExportGermanCultureTest()
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
				DataFormat = ContentFormat.Json,
				DataProperty = new DataProperty
				{
					CultureCode = "de-DE",
					SplitExportResult = true,
					PropertySource = null,
					DataProperties = new List<DataProperty>
					{
						new DataProperty {
							PropertyTarget = "NumberOne",
							PropertySource = "numberOne",
							ExportAsString = true
						},
						new DataProperty {
							PropertyTarget = "NumberTwo",
							PropertySource = "numberTwo",
							ExportAsString = true
						},
						new DataProperty {
							PropertyTarget = "NumberThree",
							PropertySource = "numberThree",
							ExportAsString = true
						},
						new DataProperty {
							PropertyTarget = "NumberFour",
							PropertySource = "numberFour",
							ExportAsString = true
						},
						new DataProperty {
							PropertyTarget = "NumberFive",
							PropertySource = "numberFive",
							ExportAsString = true
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
		public void ExportEnumerableWithPrimitiveDataType()
		{
			// Arrange
			var alpha = new AdditionalPropertyDataRoot
			{
				Beta = "One",
				Gamma = new AdditionalPropertyDataOne
				{
					Delta = "Two"
				},
				Epsilon = new List<string>
				{
					"Three",
					"Four"
				},
				Zeta = new List<AdditionalPropertyDataTwo>
				{
					new AdditionalPropertyDataTwo
					{
						Eta = "Five",
						Theta = "Six"
					},
					new AdditionalPropertyDataTwo
					{
						Eta = "Seven",
						Theta = "Eight"
					}
				}
			};

			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Json,
				DataProperty = new DataProperty
				{
					SplitExportResult = true,
					PropertySource = null,
					DataProperties = new List<DataProperty>
					{
						new DataProperty 
						{
							PropertyTarget = "Beta",
							PropertySource = "betaJson"
						},
						new DataProperty 
						{
							PropertyTarget = "Epsilon",
							PropertySource = "epsilonJson"
						},
						new DataProperty {
							PropertyTarget = "Zeta",
							PropertySource = "zetaJson",
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "Eta",
									PropertySource = "etaJson",
								},
								new DataProperty
								{
									PropertyTarget = "Theta",
									PropertySource = "thetaJson",
								}
							}
						}
					}
				}
			};

			var expectedResult = new StringBuilder();
			expectedResult.AppendLine("{");
			expectedResult.AppendLine("  \"betaJson\": \"One\",");
			expectedResult.AppendLine("  \"epsilonJson\": [");
			expectedResult.AppendLine("    \"Three\",");
			expectedResult.AppendLine("    \"Four\"");
			expectedResult.AppendLine("  ],");
			expectedResult.AppendLine("  \"zetaJson\": [");
			expectedResult.AppendLine("    {");
			expectedResult.AppendLine("      \"etaJson\": \"Five\",");
			expectedResult.AppendLine("      \"thetaJson\": \"Six\"");
			expectedResult.AppendLine("    },");
			expectedResult.AppendLine("    {");
			expectedResult.AppendLine("      \"etaJson\": \"Seven\",");
			expectedResult.AppendLine("      \"thetaJson\": \"Eight\"");
			expectedResult.AppendLine("    }");
			expectedResult.AppendLine("  ]");
			expectedResult.Append("}");

			// Act
			var result = _classUnderTest.Convert(configuration.DataProperty, new List<AdditionalPropertyDataRoot> { alpha }).ToList();

			// Assert
			result.Should().HaveCount(1);
			result.Single().Should().Be(expectedResult.ToString());
		}

		[TestMethod]
		public void ImportEnumerableWithPrimitiveDataType()
		{
			// Arrange
			var expectedResult = new AdditionalPropertyDataRoot
			{
				Beta = "One",
				Epsilon = new List<string>
				{
					"Three",
					"Four"
				},
				Zeta = new List<AdditionalPropertyDataTwo>
				{
					new AdditionalPropertyDataTwo
					{
						Eta = "Five",
						Theta = "Six"
					},
					new AdditionalPropertyDataTwo
					{
						Eta = "Seven",
						Theta = "Eight"
					}
				}
			};

			var configuration = new ConfigurationData
			{
				DataFormat = ContentFormat.Json,
				DataProperty = new DataProperty
				{
					SplitExportResult = true,
					PropertySource = null,
					DataProperties = new List<DataProperty>
					{
						new DataProperty
						{
							PropertyTarget = "Beta",
							PropertySource = "betaJson"
						},
						new DataProperty
						{
							PropertyTarget = "Epsilon",
							PropertySource = "epsilonJson"
						},
						new DataProperty {
							PropertyTarget = "Zeta",
							PropertySource = "zetaJson",
							DataProperties = new List<DataProperty>
							{
								new DataProperty
								{
									PropertyTarget = "Eta",
									PropertySource = "etaJson",
								},
								new DataProperty
								{
									PropertyTarget = "Theta",
									PropertySource = "thetaJson",
								}
							}
						}
					}
				}
			};

			var jsonData = new StringBuilder();
			jsonData.AppendLine("{");
			jsonData.AppendLine("  \"betaJson\": \"One\",");
			jsonData.AppendLine("  \"epsilonJson\": [");
			jsonData.AppendLine("    \"Three\",");
			jsonData.AppendLine("    \"Four\"");
			jsonData.AppendLine("  ],");
			jsonData.AppendLine("  \"zetaJson\": [");
			jsonData.AppendLine("    {");
			jsonData.AppendLine("      \"etaJson\": \"Five\",");
			jsonData.AppendLine("      \"thetaJson\": \"Six\"");
			jsonData.AppendLine("    },");
			jsonData.AppendLine("    {");
			jsonData.AppendLine("      \"etaJson\": \"Seven\",");
			jsonData.AppendLine("      \"thetaJson\": \"Eight\"");
			jsonData.AppendLine("    }");
			jsonData.AppendLine("  ]");
			jsonData.Append("}");

			// Act
			var result = _classUnderTest.Convert<AdditionalPropertyDataRoot>(configuration.DataProperty, jsonData.ToString()).ToList();

			// Assert
			result.Should().HaveCount(1);
			var resultItem = result.Single();
			resultItem.Should().BeEquivalentTo(expectedResult);
		}
	}
}