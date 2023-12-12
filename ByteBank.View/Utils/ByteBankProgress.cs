using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteBank.View.Utils
{
    public class ByteBankProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;
        private readonly TaskScheduler _taskScheduler;

        //ctor vai gerar o código abaixo
        public ByteBankProgress(Action<T> handler)
        {

            //Vai recuperar o taskscheduler para utilizar no report
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _handler = handler;
        }

        public void Report(T value)
        {

            //Sem se preocupar com retorno, devemos utilizar uma ação como primeiro parâmetro
            //Quando for se preocupar com retorno, podemos usar uma função.

            //Assim tiramos a responsabilidade do local onde está chamando o código
            Task.Factory.StartNew(
                    () => _handler(value),
                    System.Threading.CancellationToken.None,
                    TaskCreationOptions.None,
                    _taskScheduler
                );
        }
    }
}
