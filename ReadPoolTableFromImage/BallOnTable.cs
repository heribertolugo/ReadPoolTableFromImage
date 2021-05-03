using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace ReadPoolTableFromImage
{
    public struct BallOnTable
    {
        private IReadOnlyDictionary<int, byte> _data;
        private PointF _foundAtLocation;
        private RectangleF _calculatedLocation;
        private TableSize _tableSize;

        public BallOnTable(IDictionary<int, byte> indexedData, PointF location, TableSize tableSize)
        {
            this._data = new ReadOnlyDictionary<int, byte>(indexedData);
            this._foundAtLocation = location;
            this._tableSize = tableSize;
            this._calculatedLocation = BallOnTable.CalculateActualLocation(indexedData, tableSize);
        }

        public byte[] Data
        {
            get { return this._data.Values.ToArray(); }
        }

        public IReadOnlyDictionary<int, byte> IndexedData
        {
            get
            {
                return this._data;
            }
        }

        public PointF Location
        {
            get { return this._foundAtLocation; }
        }

        public PointF CalculatedLocation
        {
            get { return new PointF(this._calculatedLocation.X, this._calculatedLocation.Y); }
        }

        public SizeF CalculatedSize
        {
            get { return new SizeF(this._calculatedLocation.Width, this._calculatedLocation.Height); }
        }

        private static RectangleF CalculateActualLocation(IDictionary<int, byte> data, TableSize tableSize)
        {
            return new RectangleF();
        }
    }
}
