using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITEQ2.CsvHandling
{
    public class EquipmentObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _column;
        public string Column
        {
            get => _column;
            set
            {
                if (_column != value)
                {
                    _column = value;
                    OnPropertyChanged(nameof(_column));
                }
            }
        }
        private string _ggLabel;
        public string GgLabel
        {
            get => _ggLabel;
            set
            {
                if (_ggLabel != value)
                {
                    _ggLabel = value;
                    OnPropertyChanged(nameof(GgLabel));
                }
            }
        }

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        private string _make;
        public string Make
        {
            get => _make;
            set
            {
                if (_make != value)
                {
                    _make = value;
                    OnPropertyChanged(nameof(Make));
                }
            }
        }

        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                if (_model != value)
                {
                    _model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        private string _serialNo;
        public string SerialNo
        {
            get => _serialNo;
            set
            {
                if (_serialNo != value)
                {
                    _serialNo = value;
                    OnPropertyChanged(nameof(SerialNo));
                }
            }
        }

        private string _securityId;
        public string SecurityId
        {
            get => _securityId;
            set
            {
                if (_securityId != value)
                {
                    _securityId = value;
                    OnPropertyChanged(nameof(SecurityId));
                }
            }
        }

        private string _user;
        public string User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged(nameof(User));
                }
            }
        }

        private string _site;
        public string Site
        {
            get => _site;
            set
            {
                if (_site != value)
                {
                    _site = value;
                    OnPropertyChanged(nameof(Site));
                }
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private DateTime? _purchaseDate;
        public DateTime? PurchaseDate
        {
            get => _purchaseDate;
            set
            {
                if (_purchaseDate != value)
                {
                    _purchaseDate = value;
                    OnPropertyChanged(nameof(PurchaseDate));
                }
            }
        }

        private DateTime? _received;
        public DateTime? Received
        {
            get => _received;
            set
            {
                if (_received != value)
                {
                    _received = value;
                    OnPropertyChanged(nameof(Received));
                }
            }
        }

        private string _shortComment;
        public string ShortComment
        {
            get => _shortComment;
            set
            {
                if (_shortComment != value)
                {
                    _shortComment = value;
                    OnPropertyChanged(nameof(ShortComment));
                }
            }
        }

        private string _pc;
        public string PC
        {
            get => _pc;
            set
            {
                if (_pc != value)
                {
                    _pc = value;
                    OnPropertyChanged(nameof(PC));
                }
            }
        }

        private string _fucuser;
        public string FucUser
        {
            get => _fucuser;
            set
            {
                if (_fucuser != value)
                {
                    _fucuser = value;
                    OnPropertyChanged(nameof(FucUser));
                }
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private DateTime? _date;
        public DateTime? Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        private DateTime? _reportDate;
        public DateTime? ReportDate
        {
            get => _reportDate;
            set
            {
                if (_reportDate != value)
                {
                    _reportDate = value;
                    OnPropertyChanged(nameof(ReportDate));
                }
            }
        }

        private string _pcLocation;
        public string PCLocation
        {
            get => _pcLocation;
            set
            {
                if (_pcLocation != value)
                {
                    _pcLocation = value;
                    OnPropertyChanged(nameof(PCLocation));
                }
            }
        }

        private string _emplMailAdresse;
        public string EmplMailAdresse
        {
            get => _emplMailAdresse;
            set
            {
                if (_emplMailAdresse != value)
                {
                    _emplMailAdresse = value;
                    OnPropertyChanged(nameof(EmplMailAdresse));
                }
            }
        }
    }
}
