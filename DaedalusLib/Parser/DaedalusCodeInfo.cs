using System.Collections.Generic;

namespace DaedalusLib.Parser
{
    public struct TokenMatch
    {
        public string Value;
        public int Position;

        public TokenMatch(string val, int pos)
        {
            this.Value = val;
            this.Position = pos;
        }
    }

    public class Diagnostics
    {
        public int Position { get; set; }
        public string Value { get; set; }

        public Diagnostics(string value, int position)
        {
            this.Value = value;
            this.Position = position;
        }
    }

    public class DaedalusCodeInfo
    {
        /// <summary>
        /// Gets or Sets the List of 'using ...' in the code...
        /// </summary>
        public List<TokenMatch> VarDeclarations { get; set; } = new List<TokenMatch>();
        public List<TokenMatch> ConstDeclarations { get; set; } = new List<TokenMatch>();

        /// <summary>
        /// Gets or Sets the List of 'namespace ...' in the code...
        /// </summary>
        public List<TokenMatch> Functions { get; set; } = new List<TokenMatch>();

        /// <summary>
        /// Gets or Sets the List of Fields in the code...
        /// </summary>
        public List<TokenMatch> Instances { get; set; } = new List<TokenMatch>();
    }
}
