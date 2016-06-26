using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public class OrdinalOverlay<TId, TReferenced, TValue> : IOverlay<TId, TReferenced, TValue>
        where TId : IOrdinalId<TId>
        where TReferenced : IIdReferenced<TId>
    {
        private List<TValue> values = new List<TValue>();

        public OrdinalOverlay(Func<TValue> defaultValueFactory = null)
        {
            this.DefaultValueFactory = defaultValueFactory;
        }

        public Func<TValue> DefaultValueFactory { get; set; }

        public TValue this[TReferenced referenced]
        {
            get { return this[referenced.Id]; }
            set { this[referenced.Id] = value; }
        }

        public TValue this[TId id]
        {
            get
            {
                this.ExtendIfNeeded(id.Value);
                return this.values[id.Value];
            }

            set
            {
                this.ExtendIfNeeded(id.Value);
                this.values[id.Value] = value;
            }
        }

        private void ExtendIfNeeded(int index)
        {
            if (index + 1 > this.values.Count)
            {
                int addedCount = index + 1 - this.values.Count;

                if (this.DefaultValueFactory == null)
                {
                    this.values.AddRange(Enumerable.Repeat(default(TValue), addedCount));
                }
                else
                {
                    for (int i = 0; i < addedCount; i++)
                    {
                        this.values.Add(this.DefaultValueFactory());
                    }
                }
            }
        }
    }
}
