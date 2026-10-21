using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using System.Xml.Serialization;

namespace NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual
{
    /// <summary>
    ///     NB01 - Grupo do ICMS Previsto no Pagamento Antecipado (NT 2026.008)
    ///     <para>Usado só no modelo 55, quando tpNFDebito = 06 (pagamento antecipado) ou tpOperGov = 4
    ///     (recebimento do pagamento com fornecimento posterior). Na NFC-e gera Rejeição 1280.</para>
    /// </summary>
    public class ICMSPrevistoPagtoAntecip : ICMSBasico
    {
        private decimal _vICMSPrevisto;

        /// <summary>
        ///     NB02 - Valor do ICMS que incidirá no momento do fornecimento futuro
        ///     <para>Quando não houver ICMS devido no fornecimento, informar 0.</para>
        /// </summary>
        [XmlElement(Order = 1)]
        public decimal vICMSPrevisto
        {
            get { return _vICMSPrevisto.Arredondar(2); }
            set { _vICMSPrevisto = value.Arredondar(2); }
        }
    }
}
