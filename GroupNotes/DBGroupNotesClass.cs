using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;

namespace GroupNotes
{
    class Group
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public int Count { get; set; }
        public int Id_User { get; set; }
    }
    class Note
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateComplet { get; set; }
        public bool Complet { get; set; }
        public int Id_User { get; set; }
        public int Id_Group { get; set; }
    }
    class User
    {
        [Key]
        public int Id { get; set; }
        public string FIO { get; set; }
        public DateTime Birthday { get; set; }
        public string Login { get; set; }
    }
    class ShowNote : INotifyPropertyChanged
    {
        int id;
        string text;
        DateTime dateCreate;
        DateTime dateComplet;
        bool complet;
        string color;
        string name;

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }
        public DateTime DateCreate
        {
            get { return dateCreate.Date; }
            set
            {
                dateCreate = value;
                OnPropertyChanged("DateCreate");
            }
        }
        public DateTime DateComplet
        {
            get { return dateComplet.Date; }
            set
            {
                dateComplet = value;
                OnPropertyChanged("DateComplet");
            }
        }
        public bool Complet
        {
            get { return complet; }
            set
            {
                complet = value;
                OnPropertyChanged("Complet");
            }
        }
        public string Color
        {
            get { return color; }
            set
            {
                color = value;
                OnPropertyChanged("Color");
            }
        }
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
