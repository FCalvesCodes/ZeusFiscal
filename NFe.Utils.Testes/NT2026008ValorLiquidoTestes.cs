using System.Linq;
using System.Xml.Linq;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Utils.NFe;
using Xunit;

namespace NFe.Utils.Testes
{
    /// <summary>
    ///     NT 2026.008 - Valor líquido do produto (I11a/I11b/I11c, W01a) e ICMS previsto no
    ///     pagamento antecipado (NB01). Garante que os campos sobrevivem à leitura e escrita do XML.
    /// </summary>
    public class NT2026008ValorLiquidoTestes
    {
        private static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

        private const string Prod =
            "<prod><cProd>1</cProd><cEAN>SEM GTIN</cEAN><xProd>PRODUTO</xProd><NCM>44079990</NCM><CFOP>5102</CFOP>" +
            "<uCom>UN</uCom><qCom>2.0000</qCom><vUnCom>50.0000000000</vUnCom><vProd>100.00</vProd>{0}" +
            "<cEANTrib>SEM GTIN</cEANTrib><uTrib>UN</uTrib><qTrib>2.0000</qTrib><vUnTrib>50.0000000000</vUnTrib><indTot>1</indTot></prod>";

        private const string IcmsSn102 = "<ICMSSN102><orig>0</orig><CSOSN>102</CSOSN></ICMSSN102>";

        private static string MontarNFe(string camposLiquidos, string icms, string totalLiquido)
        {
            return "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"NFe51261044585342000196550010000000011000000010\" versao=\"4.00\">" +
                   "<ide><cUF>51</cUF><cNF>00000001</cNF><natOp>VENDA</natOp><mod>55</mod><serie>1</serie><nNF>1</nNF>" +
                   "<dhEmi>2026-10-21T10:00:00-04:00</dhEmi><tpNF>1</tpNF><idDest>1</idDest><cMunFG>5103403</cMunFG><tpImp>1</tpImp>" +
                   "<tpEmis>1</tpEmis><cDV>0</cDV><tpAmb>2</tpAmb><finNFe>1</finNFe><indFinal>1</indFinal><indPres>1</indPres>" +
                   "<procEmi>0</procEmi><verProc>1.0</verProc></ide>" +
                   "<emit><CNPJ>44585342000196</CNPJ><xNome>EMITENTE</xNome><enderEmit><xLgr>RUA</xLgr><nro>1</nro><xBairro>CENTRO</xBairro>" +
                   "<cMun>5103403</cMun><xMun>CUIABA</xMun><UF>MT</UF><CEP>78000000</CEP></enderEmit><IE>139120149</IE><CRT>1</CRT></emit>" +
                   "<det nItem=\"1\">" + string.Format(Prod, camposLiquidos) +
                   "<imposto><ICMS>" + icms + "</ICMS></imposto></det>" +
                   "<total>" + totalLiquido + "<ICMSTot><vBC>0.00</vBC><vICMS>0.00</vICMS><vICMSDeson>0.00</vICMSDeson><vFCP>0.00</vFCP>" +
                   "<vBCST>0.00</vBCST><vST>0.00</vST><vFCPST>0.00</vFCPST><vFCPSTRet>0.00</vFCPSTRet><vProd>100.00</vProd><vFrete>0.00</vFrete>" +
                   "<vSeg>0.00</vSeg><vDesc>0.00</vDesc><vII>0.00</vII><vIPI>0.00</vIPI><vIPIDevol>0.00</vIPIDevol><vPIS>0.00</vPIS>" +
                   "<vCOFINS>0.00</vCOFINS><vOutro>0.00</vOutro><vNF>100.00</vNF></ICMSTot></total>" +
                   "<transp><modFrete>9</modFrete></transp><pag><detPag><tPag>01</tPag><vPag>100.00</vPag></detPag></pag>" +
                   "</infNFe></NFe>";
        }

        private static XDocument IdaEVolta(string xml)
        {
            var nfe = new Classes.NFe().CarregarDeXmlString(xml, false);
            return XDocument.Parse(nfe.ObterXmlString());
        }

        [Fact]
        public void ValoresLiquidosDevemSerPreservadosLogoAposVProd()
        {
            var xml = MontarNFe("<vUnComLiq>44.0000000000</vUnComLiq><vProdLiq>88.00</vProdLiq>", IcmsSn102,
                "<vProdLiqTot>88.00</vProdLiqTot>");

            var doc = IdaEVolta(xml);

            var prod = doc.Descendants(Ns + "prod").Single();
            var nomes = prod.Elements().Select(e => e.Name.LocalName).ToList();
            var posVProd = nomes.IndexOf("vProd");
            Assert.Equal("vUnComLiq", nomes[posVProd + 1]);
            Assert.Equal("vProdLiq", nomes[posVProd + 2]);
            Assert.Equal("cEANTrib", nomes[posVProd + 3]);
            Assert.Equal(44m, decimal.Parse(prod.Element(Ns + "vUnComLiq").Value, System.Globalization.CultureInfo.InvariantCulture));
            Assert.Equal("88.00", prod.Element(Ns + "vProdLiq").Value);
        }

        [Fact]
        public void TotalLiquidoDeveSerOPrimeiroFilhoDeTotal()
        {
            var xml = MontarNFe("<vUnComLiq>44.0000000000</vUnComLiq><vProdLiq>88.00</vProdLiq>", IcmsSn102,
                "<vProdLiqTot>88.00</vProdLiqTot>");

            var total = IdaEVolta(xml).Descendants(Ns + "total").Single();

            var primeiro = total.Elements().First();
            Assert.Equal("vProdLiqTot", primeiro.Name.LocalName);
            Assert.Equal("88.00", primeiro.Value);
            Assert.Equal("ICMSTot", total.Elements().ElementAt(1).Name.LocalName);
        }

        [Fact]
        public void CamposNaoInformadosNaoDevemSerSerializados()
        {
            var doc = IdaEVolta(MontarNFe(string.Empty, IcmsSn102, string.Empty));

            Assert.Empty(doc.Descendants(Ns + "vUnComLiq"));
            Assert.Empty(doc.Descendants(Ns + "vProdLiq"));
            Assert.Empty(doc.Descendants(Ns + "vProdLiqTot"));
        }

        [Fact]
        public void IcmsPrevistoNoPagamentoAntecipadoDeveSerPreservado()
        {
            var xml = MontarNFe(string.Empty,
                "<ICMSPrevistoPagtoAntecip><vICMSPrevisto>12.34</vICMSPrevisto></ICMSPrevistoPagtoAntecip>", string.Empty);

            var nfe = new Classes.NFe().CarregarDeXmlString(xml, false);
            var icms = Assert.IsType<ICMSPrevistoPagtoAntecip>(nfe.infNFe.det[0].imposto.ICMS.TipoICMS);
            Assert.Equal(12.34m, icms.vICMSPrevisto);

            var grupo = XDocument.Parse(nfe.ObterXmlString()).Descendants(Ns + "ICMS").Single().Elements().Single();
            Assert.Equal("ICMSPrevistoPagtoAntecip", grupo.Name.LocalName);
            Assert.Equal("12.34", grupo.Element(Ns + "vICMSPrevisto").Value);
        }

        [Fact]
        public void IcmsPrevistoZeradoDeveSerSerializado()
        {
            var xml = MontarNFe(string.Empty,
                "<ICMSPrevistoPagtoAntecip><vICMSPrevisto>0.00</vICMSPrevisto></ICMSPrevistoPagtoAntecip>", string.Empty);

            var doc = IdaEVolta(xml);

            Assert.Equal("0.00", doc.Descendants(Ns + "vICMSPrevisto").Single().Value);
        }
    }
}
