using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Eshava.Test.Transition.Models;
using Eshava.Transition.Engines;
using Eshava.Transition.Enums;
using Eshava.Transition.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Transition.Engines
{
	[TestClass, TestCategory("Engines")]
	public class XMLConverterEngineTests
	{
		private XMLConverterEngine _classUnderTest;
		private string _inputFile = @"..\..\..\Input\Addresses.xml";
		private string _inputFile2 = @"..\..\..\Input\Addresses2.xml";

		[TestInitialize]
		public void Setup()
		{
			_classUnderTest = new XMLConverterEngine();
		}

		private ConfigurationData GetImportConfiguration(bool setAddressTarget = false, bool setAddressSource = true, bool splitExport = false)
		{
			return new ConfigurationData
			{
				DataFormat = ContentFormat.Xml,
				DataProperty = new DataProperty
				{
					SplitExportResult = splitExport,
					PropertySource = "adressen",
					DataProperties = new List<DataProperty> {
						new DataProperty
						{
							PropertySource = "adresse",
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
										new DataProperty {
											PropertyTarget = "Street",
											PropertySource = "strasse"
										},
										new DataProperty {
											PropertyTarget = "ZIPCode",
											PropertySource = "plz"
										},
										new DataProperty {
											PropertyTarget = "Place",
											PropertySource = "ort"
										},
										new DataProperty {
											PropertyTarget = "Country",
											PropertySource = "land"
										}
									},
								},
								new DataProperty {
									PropertySource = "kommunikationen",
									PropertyTarget = "Communications",
									MappingProperties = new List<string>{
											"Value",
											"Type"
										},
									DataProperties = new List<DataProperty> {
										new DataProperty {
											PropertySource = "kommunikation",
											DataProperties = new List<DataProperty> {
												 new DataProperty {
													PropertyTarget = "Type",
													PropertySource = "type"
												 },
												 new DataProperty {
													PropertyTarget = "Value",
													PropertySource = "wert"
												 }
											}
										}
									},
								},
								new DataProperty {
									PropertySource = "ansprechpartner",
									PropertyTarget = "Contacts",
									MappingProperties = new List<string>{
											"FirstName",
											"LastName"
									},
									DataProperties = new List<DataProperty> {
										new DataProperty {
											PropertySource = "kontakt",
											DataProperties = new List<DataProperty> {
												new DataProperty {
													PropertyTarget = "FirstName",
													PropertySource = "vorname"
												},
												new DataProperty {
													PropertyTarget = "LastName",
													PropertySource = "nachname"
												},
												new DataProperty {
													PropertyTarget = "Title",
													PropertySource = "anrede"
												},
												new DataProperty {
													PropertyTarget = "Position",
													PropertySource = "position"
												},
												new DataProperty {
													PropertySource = "kommunikationen",
													PropertyTarget = "Communications",
													MappingProperties = new List<string>{
															"Value",
															"Type"
													},
													DataProperties = new List<DataProperty> {
														new DataProperty {
															PropertySource = "kommunikation",
															DataProperties = new List<DataProperty> {
																 new DataProperty {
																	PropertyTarget = "Type",
																	PropertySource = "type"
																 },
																 new DataProperty {
																	PropertyTarget = "Value",
																	PropertySource = "wert"
																 }
															}
														}
													}
												}
											}
										},
									}
								}
							}
						}
					}
				}
			};
		}

		[TestMethod]
		public void XMLConvertWithoutRemoveDoubletsTest()
		{
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfiguration();
			var addresses = _classUnderTest.Convert<Address>(configuration.DataProperty, data, false).ToList();

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
		public void XMLConvertWithRemoveDoubletsTest()
		{
			var data = File.ReadAllText(_inputFile, Encoding.UTF8);
			var configuration = GetImportConfiguration();
			var addresses = _classUnderTest.Convert<Address>(configuration.DataProperty, data).ToList();

			addresses.Should().HaveCount(3);
			addresses[0].Communications.Should().HaveCount(2);
			addresses[0].Contacts.Should().HaveCount(2);

			addresses[1].Communications.Should().HaveCount(0);
			addresses[1].Contacts.Should().HaveCount(1);

			addresses[2].Communications.Should().HaveCount(1);
			addresses[2].Contacts.Should().HaveCount(0);
		}

		[TestMethod]
		public void XMLConvertCompanyWithoutRemoveDoubletsFromSource2Test()
		{
			var data = File.ReadAllText(_inputFile2, Encoding.UTF8);
			var configuration = GetImportConfiguration(true, false);
			var addresses = _classUnderTest.Convert<Company>(configuration.DataProperty, data, false).ToList();

			addresses.Should().HaveCount(4);
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

			addresses[2].Communications.Should().HaveCount(0);
			addresses[2].Contacts.Should().HaveCount(2);
			addresses[2].AddressNumber.Should().Be("Kdn0001");
			addresses[2].CompanyName.Should().Be("Alphabet");
			addresses[2].Address.Street.Should().Be("ABC Allee 123");
			addresses[2].Address.Place.Should().Be("Digenskirchen");
			addresses[2].Address.ZIPCode.Should().Be(12345);
			addresses[2].Address.Country.Should().Be("Deutschland");
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
			addresses[3].Address.Street.Should().Be("Fun Road 666");
			addresses[3].Address.Place.Should().Be("Entenhausen");
			addresses[3].Address.ZIPCode.Should().Be(98765);
			addresses[3].Address.Country.Should().Be("Deutschland");
			addresses[3].Communications.First().Type.Should().Be("mail");
			addresses[3].Communications.First().Value.Should().Be("contact@spielwaren-gmbh.de");
		}

		[TestMethod]
		public void XMLExportConvertTest()
		{
			var data = _classUnderTest.Convert<Address>(GetImportConfiguration().DataProperty, File.ReadAllText(_inputFile, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration();
			var xmlData = _classUnderTest.Convert(configuration.DataProperty, data).ToArray();

			xmlData.Length.Should().Be(1);

			var expectedResult = new StringBuilder();
			expectedResult.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
			expectedResult.AppendLine("<adressen>");
			expectedResult.AppendLine("  <adresse>");
			expectedResult.AppendLine("    <kundennummer>Kdn0001</kundennummer>");
			expectedResult.AppendLine("    <unternehmen>Alphabet</unternehmen>");
			expectedResult.AppendLine("    <anschrift>");
			expectedResult.AppendLine("      <strasse>ABC Allee 123</strasse>");
			expectedResult.AppendLine("      <plz>12345</plz>");
			expectedResult.AppendLine("      <ort>Digenskirchen</ort>");
			expectedResult.AppendLine("      <land>Deutschland</land>");
			expectedResult.AppendLine("    </anschrift>");
			expectedResult.AppendLine("    <kommunikationen>");
			expectedResult.AppendLine("      <kommunikation>");
			expectedResult.AppendLine("        <type>mail</type>");
			expectedResult.AppendLine("        <wert>info@alphabet.de</wert>");
			expectedResult.AppendLine("      </kommunikation>");
			expectedResult.AppendLine("      <kommunikation>");
			expectedResult.AppendLine("        <type>phone</type>");
			expectedResult.AppendLine("        <wert>+4985 123450</wert>");
			expectedResult.AppendLine("      </kommunikation>");
			expectedResult.AppendLine("    </kommunikationen>");
			expectedResult.AppendLine("    <ansprechpartner>");
			expectedResult.AppendLine("      <kontakt>");
			expectedResult.AppendLine("        <vorname>Holger</vorname>");
			expectedResult.AppendLine("        <nachname>Wastow</nachname>");
			expectedResult.AppendLine("        <anrede>Herr</anrede>");
			expectedResult.AppendLine("        <position>F&amp;E</position>");
			expectedResult.AppendLine("        <kommunikationen>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mail</type>");
			expectedResult.AppendLine("            <wert>h.wastow@alphabet.de</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>phone</type>");
			expectedResult.AppendLine("            <wert>+4985 1234567</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mobile</type>");
			expectedResult.AppendLine("            <wert>+49151 89898989</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("        </kommunikationen>");
			expectedResult.AppendLine("      </kontakt>");
			expectedResult.AppendLine("      <kontakt>");
			expectedResult.AppendLine("        <vorname>Emmen</vorname>");
			expectedResult.AppendLine("        <nachname>Taler</nachname>");
			expectedResult.AppendLine("        <anrede>Frau</anrede>");
			expectedResult.AppendLine("        <position>Kundenservice</position>");
			expectedResult.AppendLine("        <kommunikationen>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mail</type>");
			expectedResult.AppendLine("            <wert>e.taler@alphabet.de</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>phone</type>");
			expectedResult.AppendLine("            <wert>+4985 1234589</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("        </kommunikationen>");
			expectedResult.AppendLine("      </kontakt>");
			expectedResult.AppendLine("    </ansprechpartner>");
			expectedResult.AppendLine("  </adresse>");
			expectedResult.AppendLine("  <adresse>");
			expectedResult.AppendLine("    <kundennummer>Kdn0003</kundennummer>");
			expectedResult.AppendLine("    <unternehmen>TWC AG</unternehmen>");
			expectedResult.AppendLine("    <anschrift>");
			expectedResult.AppendLine("      <strasse>Deep Link Avenue 1</strasse>");
			expectedResult.AppendLine("      <plz>45110</plz>");
			expectedResult.AppendLine("      <ort>Pirna</ort>");
			expectedResult.AppendLine("      <land>Deutschland</land>");
			expectedResult.AppendLine("    </anschrift>");
			expectedResult.AppendLine("    <ansprechpartner>");
			expectedResult.AppendLine("      <kontakt>");
			expectedResult.AppendLine("        <vorname>Lasmiranda</vorname>");
			expectedResult.AppendLine("        <nachname>Dennsiewillja</nachname>");
			expectedResult.AppendLine("        <anrede>Frau</anrede>");
			expectedResult.AppendLine("        <position>Sonstiges</position>");
			expectedResult.AppendLine("        <kommunikationen>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mail</type>");
			expectedResult.AppendLine("            <wert>l.d@twc-ag.net</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("        </kommunikationen>");
			expectedResult.AppendLine("      </kontakt>");
			expectedResult.AppendLine("    </ansprechpartner>");
			expectedResult.AppendLine("  </adresse>");
			expectedResult.AppendLine("  <adresse>");
			expectedResult.AppendLine("    <kundennummer>Kdn0002</kundennummer>");
			expectedResult.AppendLine("    <unternehmen>Spielwaren GmbH</unternehmen>");
			expectedResult.AppendLine("    <anschrift>");
			expectedResult.AppendLine("      <strasse>Fun Road 666</strasse>");
			expectedResult.AppendLine("      <plz>98765</plz>");
			expectedResult.AppendLine("      <ort>Entenhausen</ort>");
			expectedResult.AppendLine("      <land>Deutschland</land>");
			expectedResult.AppendLine("    </anschrift>");
			expectedResult.AppendLine("    <kommunikationen>");
			expectedResult.AppendLine("      <kommunikation>");
			expectedResult.AppendLine("        <type>mail</type>");
			expectedResult.AppendLine("        <wert>contact@spielwaren-gmbh.de</wert>");
			expectedResult.AppendLine("      </kommunikation>");
			expectedResult.AppendLine("    </kommunikationen>");
			expectedResult.AppendLine("  </adresse>");
			expectedResult.Append("</adressen>");

			var expectedDocument = new XmlDocument();
			expectedDocument.LoadXml(expectedResult.ToString());

			xmlData[0].Should().Be(expectedDocument.OuterXml);
		}

		[TestMethod]
		public void XMLExportConvertForCompanyTest()
		{
			var data = _classUnderTest.Convert<Company>(GetImportConfiguration(true, false).DataProperty, File.ReadAllText(_inputFile2, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration(true, false);
			var xmlData = _classUnderTest.Convert(configuration.DataProperty, data).ToArray();

			xmlData.Length.Should().Be(1);

			var expectedResult = new StringBuilder();
			expectedResult.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
			expectedResult.AppendLine("<adressen>");
			expectedResult.AppendLine("  <adresse>");
			expectedResult.AppendLine("    <unternehmen>Alphabet</unternehmen>");
			expectedResult.AppendLine("    <strasse>ABC Allee 123</strasse>");
			expectedResult.AppendLine("    <plz>12345</plz>");
			expectedResult.AppendLine("    <ort>Digenskirchen</ort>");
			expectedResult.AppendLine("    <land>Deutschland</land>");
			expectedResult.AppendLine("    <kundennummer>Kdn0001</kundennummer>");
			expectedResult.AppendLine("    <kommunikationen>");
			expectedResult.AppendLine("      <kommunikation>");
			expectedResult.AppendLine("        <type>mail</type>");
			expectedResult.AppendLine("        <wert>info@alphabet.de</wert>");
			expectedResult.AppendLine("      </kommunikation>");
			expectedResult.AppendLine("      <kommunikation>");
			expectedResult.AppendLine("        <type>phone</type>");
			expectedResult.AppendLine("        <wert>+4985 123450</wert>");
			expectedResult.AppendLine("      </kommunikation>");
			expectedResult.AppendLine("    </kommunikationen>");
			expectedResult.AppendLine("    <ansprechpartner>");
			expectedResult.AppendLine("      <kontakt>");
			expectedResult.AppendLine("        <vorname>Holger</vorname>");
			expectedResult.AppendLine("        <nachname>Wastow</nachname>");
			expectedResult.AppendLine("        <anrede>Herr</anrede>");
			expectedResult.AppendLine("        <position>F&amp;E</position>");
			expectedResult.AppendLine("        <kommunikationen>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mail</type>");
			expectedResult.AppendLine("            <wert>h.wastow@alphabet.de</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>phone</type>");
			expectedResult.AppendLine("            <wert>+4985 1234567</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mobile</type>");
			expectedResult.AppendLine("            <wert>+49151 89898989</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("        </kommunikationen>");
			expectedResult.AppendLine("      </kontakt>");
			expectedResult.AppendLine("      <kontakt>");
			expectedResult.AppendLine("        <vorname>Emmen</vorname>");
			expectedResult.AppendLine("        <nachname>Taler</nachname>");
			expectedResult.AppendLine("        <anrede>Frau</anrede>");
			expectedResult.AppendLine("        <position>Kundenservice</position>");
			expectedResult.AppendLine("        <kommunikationen>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mail</type>");
			expectedResult.AppendLine("            <wert>e.taler@alphabet.de</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>phone</type>");
			expectedResult.AppendLine("            <wert>+4985 1234589</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("        </kommunikationen>");
			expectedResult.AppendLine("      </kontakt>");
			expectedResult.AppendLine("    </ansprechpartner>");
			expectedResult.AppendLine("  </adresse>");
			expectedResult.AppendLine("  <adresse>");
			expectedResult.AppendLine("    <unternehmen>TWC AG</unternehmen>");
			expectedResult.AppendLine("    <strasse>Deep Link Avenue 1</strasse>");
			expectedResult.AppendLine("    <plz>45110</plz>");
			expectedResult.AppendLine("    <ort>Pirna</ort>");
			expectedResult.AppendLine("    <land>Deutschland</land>");
			expectedResult.AppendLine("    <kundennummer>Kdn0003</kundennummer>");
			expectedResult.AppendLine("    <ansprechpartner>");
			expectedResult.AppendLine("      <kontakt>");
			expectedResult.AppendLine("        <vorname>Lasmiranda</vorname>");
			expectedResult.AppendLine("        <nachname>Dennsiewillja</nachname>");
			expectedResult.AppendLine("        <anrede>Frau</anrede>");
			expectedResult.AppendLine("        <position>Sonstiges</position>");
			expectedResult.AppendLine("        <kommunikationen>");
			expectedResult.AppendLine("          <kommunikation>");
			expectedResult.AppendLine("            <type>mail</type>");
			expectedResult.AppendLine("            <wert>l.d@twc-ag.net</wert>");
			expectedResult.AppendLine("          </kommunikation>");
			expectedResult.AppendLine("        </kommunikationen>");
			expectedResult.AppendLine("      </kontakt>");
			expectedResult.AppendLine("    </ansprechpartner>");
			expectedResult.AppendLine("  </adresse>");
			expectedResult.AppendLine("  <adresse>");
			expectedResult.AppendLine("    <unternehmen>Spielwaren GmbH</unternehmen>");
			expectedResult.AppendLine("    <strasse>Fun Road 666</strasse>");
			expectedResult.AppendLine("    <plz>98765</plz>");
			expectedResult.AppendLine("    <ort>Entenhausen</ort>");
			expectedResult.AppendLine("    <land>Deutschland</land>");
			expectedResult.AppendLine("    <kundennummer>Kdn0002</kundennummer>");
			expectedResult.AppendLine("    <kommunikationen>");
			expectedResult.AppendLine("      <kommunikation>");
			expectedResult.AppendLine("        <type>mail</type>");
			expectedResult.AppendLine("        <wert>contact@spielwaren-gmbh.de</wert>");
			expectedResult.AppendLine("      </kommunikation>");
			expectedResult.AppendLine("    </kommunikationen>");
			expectedResult.AppendLine("  </adresse>");
			expectedResult.Append("</adressen>");

			var expectedDocument = new XmlDocument();
			expectedDocument.LoadXml(expectedResult.ToString());

			xmlData[0].Should().Be(expectedDocument.OuterXml);
		}

		[TestMethod]
		public void XMLExportConvertForCompanySplitExportTest()
		{
			var data = _classUnderTest.Convert<Company>(GetImportConfiguration(true, false).DataProperty, File.ReadAllText(_inputFile2, Encoding.UTF8), true).ToList();
			var configuration = GetImportConfiguration(true, false, true);
			var xmlData = _classUnderTest.Convert(configuration.DataProperty, data).ToArray();

			xmlData.Length.Should().Be(3);

			var expectedResultOne = new StringBuilder();
			expectedResultOne.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
			expectedResultOne.AppendLine("<adressen>");
			expectedResultOne.AppendLine("  <adresse>");
			expectedResultOne.AppendLine("    <unternehmen>Alphabet</unternehmen>");
			expectedResultOne.AppendLine("    <strasse>ABC Allee 123</strasse>");
			expectedResultOne.AppendLine("    <plz>12345</plz>");
			expectedResultOne.AppendLine("    <ort>Digenskirchen</ort>");
			expectedResultOne.AppendLine("    <land>Deutschland</land>");
			expectedResultOne.AppendLine("    <kundennummer>Kdn0001</kundennummer>");
			expectedResultOne.AppendLine("    <kommunikationen>");
			expectedResultOne.AppendLine("      <kommunikation>");
			expectedResultOne.AppendLine("        <type>mail</type>");
			expectedResultOne.AppendLine("        <wert>info@alphabet.de</wert>");
			expectedResultOne.AppendLine("      </kommunikation>");
			expectedResultOne.AppendLine("      <kommunikation>");
			expectedResultOne.AppendLine("        <type>phone</type>");
			expectedResultOne.AppendLine("        <wert>+4985 123450</wert>");
			expectedResultOne.AppendLine("      </kommunikation>");
			expectedResultOne.AppendLine("    </kommunikationen>");
			expectedResultOne.AppendLine("    <ansprechpartner>");
			expectedResultOne.AppendLine("      <kontakt>");
			expectedResultOne.AppendLine("        <vorname>Holger</vorname>");
			expectedResultOne.AppendLine("        <nachname>Wastow</nachname>");
			expectedResultOne.AppendLine("        <anrede>Herr</anrede>");
			expectedResultOne.AppendLine("        <position>F&amp;E</position>");
			expectedResultOne.AppendLine("        <kommunikationen>");
			expectedResultOne.AppendLine("          <kommunikation>");
			expectedResultOne.AppendLine("            <type>mail</type>");
			expectedResultOne.AppendLine("            <wert>h.wastow@alphabet.de</wert>");
			expectedResultOne.AppendLine("          </kommunikation>");
			expectedResultOne.AppendLine("          <kommunikation>");
			expectedResultOne.AppendLine("            <type>phone</type>");
			expectedResultOne.AppendLine("            <wert>+4985 1234567</wert>");
			expectedResultOne.AppendLine("          </kommunikation>");
			expectedResultOne.AppendLine("          <kommunikation>");
			expectedResultOne.AppendLine("            <type>mobile</type>");
			expectedResultOne.AppendLine("            <wert>+49151 89898989</wert>");
			expectedResultOne.AppendLine("          </kommunikation>");
			expectedResultOne.AppendLine("        </kommunikationen>");
			expectedResultOne.AppendLine("      </kontakt>");
			expectedResultOne.AppendLine("      <kontakt>");
			expectedResultOne.AppendLine("        <vorname>Emmen</vorname>");
			expectedResultOne.AppendLine("        <nachname>Taler</nachname>");
			expectedResultOne.AppendLine("        <anrede>Frau</anrede>");
			expectedResultOne.AppendLine("        <position>Kundenservice</position>");
			expectedResultOne.AppendLine("        <kommunikationen>");
			expectedResultOne.AppendLine("          <kommunikation>");
			expectedResultOne.AppendLine("            <type>mail</type>");
			expectedResultOne.AppendLine("            <wert>e.taler@alphabet.de</wert>");
			expectedResultOne.AppendLine("          </kommunikation>");
			expectedResultOne.AppendLine("          <kommunikation>");
			expectedResultOne.AppendLine("            <type>phone</type>");
			expectedResultOne.AppendLine("            <wert>+4985 1234589</wert>");
			expectedResultOne.AppendLine("          </kommunikation>");
			expectedResultOne.AppendLine("        </kommunikationen>");
			expectedResultOne.AppendLine("      </kontakt>");
			expectedResultOne.AppendLine("    </ansprechpartner>");
			expectedResultOne.AppendLine("  </adresse>");
			expectedResultOne.Append("</adressen>");

			var expectedResultTwo = new StringBuilder();
			expectedResultTwo.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
			expectedResultTwo.AppendLine("<adressen>");
			expectedResultTwo.AppendLine("  <adresse>");
			expectedResultTwo.AppendLine("    <unternehmen>TWC AG</unternehmen>");
			expectedResultTwo.AppendLine("    <strasse>Deep Link Avenue 1</strasse>");
			expectedResultTwo.AppendLine("    <plz>45110</plz>");
			expectedResultTwo.AppendLine("    <ort>Pirna</ort>");
			expectedResultTwo.AppendLine("    <land>Deutschland</land>");
			expectedResultTwo.AppendLine("    <kundennummer>Kdn0003</kundennummer>");
			expectedResultTwo.AppendLine("    <ansprechpartner>");
			expectedResultTwo.AppendLine("      <kontakt>");
			expectedResultTwo.AppendLine("        <vorname>Lasmiranda</vorname>");
			expectedResultTwo.AppendLine("        <nachname>Dennsiewillja</nachname>");
			expectedResultTwo.AppendLine("        <anrede>Frau</anrede>");
			expectedResultTwo.AppendLine("        <position>Sonstiges</position>");
			expectedResultTwo.AppendLine("        <kommunikationen>");
			expectedResultTwo.AppendLine("          <kommunikation>");
			expectedResultTwo.AppendLine("            <type>mail</type>");
			expectedResultTwo.AppendLine("            <wert>l.d@twc-ag.net</wert>");
			expectedResultTwo.AppendLine("          </kommunikation>");
			expectedResultTwo.AppendLine("        </kommunikationen>");
			expectedResultTwo.AppendLine("      </kontakt>");
			expectedResultTwo.AppendLine("    </ansprechpartner>");
			expectedResultTwo.AppendLine("  </adresse>");
			expectedResultTwo.Append("</adressen>");

			var expectedResultThree = new StringBuilder();
			expectedResultThree.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
			expectedResultThree.AppendLine("<adressen>");
			expectedResultThree.AppendLine("  <adresse>");
			expectedResultThree.AppendLine("    <unternehmen>Spielwaren GmbH</unternehmen>");
			expectedResultThree.AppendLine("    <strasse>Fun Road 666</strasse>");
			expectedResultThree.AppendLine("    <plz>98765</plz>");
			expectedResultThree.AppendLine("    <ort>Entenhausen</ort>");
			expectedResultThree.AppendLine("    <land>Deutschland</land>");
			expectedResultThree.AppendLine("    <kundennummer>Kdn0002</kundennummer>");
			expectedResultThree.AppendLine("    <kommunikationen>");
			expectedResultThree.AppendLine("      <kommunikation>");
			expectedResultThree.AppendLine("        <type>mail</type>");
			expectedResultThree.AppendLine("        <wert>contact@spielwaren-gmbh.de</wert>");
			expectedResultThree.AppendLine("      </kommunikation>");
			expectedResultThree.AppendLine("    </kommunikationen>");
			expectedResultThree.AppendLine("  </adresse>");
			expectedResultThree.Append("</adressen>");

			var expectedDocumentOne = new XmlDocument();
			expectedDocumentOne.LoadXml(expectedResultOne.ToString());

			var expectedDocumentTwo = new XmlDocument();
			expectedDocumentTwo.LoadXml(expectedResultTwo.ToString());

			var expectedDocumentThree = new XmlDocument();
			expectedDocumentThree.LoadXml(expectedResultThree.ToString());

			xmlData[0].Should().Be(expectedDocumentOne.OuterXml);
			xmlData[1].Should().Be(expectedDocumentTwo.OuterXml);
			xmlData[2].Should().Be(expectedDocumentThree.OuterXml);
		}
	}
}