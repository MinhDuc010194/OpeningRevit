using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonTools.OpeningClient.Validation
{
    public class ValidationData
    {
        private LocalDataPushModel _data;

        public ValidationData(LocalDataPushModel localDataPushModel)
        {
            _data = localDataPushModel;
        }

        public void ImpelementValidate()
        {
            PullValidate();
        }

        private bool PullValidate()
        {
            if (_data.OpeningsLocalPullAction.Count < 1) {
                return true;
            }
            int count = TurnPullElementToPush();
            if (count > 0) {
                MessageBox.Show(CommonTools.OpeningClient.Support.DefineMessage.HasOpeningPullButCantCreate + count);
            }
            return true;
        }

        private bool PushValidate()
        {
            return true;
        }

        private int TurnPullElementToPush()
        {
            var elementsCantCreated = _data.OpeningsLocalPullAction.Where(x => x.ServerStatus.Equals(DefineStatus.NORMAL)
              && x.LocalStatus.Equals(DefineStatus.NORMAL)
              && string.IsNullOrEmpty(x.IdRevitElement)).ToList();
            if (elementsCantCreated.Count < 1) {
                return 0;
            }

            foreach (var element in elementsCantCreated) {
                element.LocalStatus = DefineStatus.DELETED;
                _data.OpeningsLocalPullAction.Remove(element);
                _data.OpeningsLocalPushAction.Add(element);
            }
            return elementsCantCreated.Count;
        }
    }
}