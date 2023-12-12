using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {

            BtnProcessar.IsEnabled = false;

            var contas = r_Repositorio.GetContaClientes();

            PgsProgresso.Maximum = contas.Count();

            LimparView();

            var inicio = DateTime.Now;


            // Nós criamos o ByteBankProgress mas já existe uma função nativa que faz a mesma coisa,
            //no caso o Progress
            //var byteBankProgress = new ByteBankProgress<string>(str => PgsProgresso.Value++);
            var progress = new Progress<string>(str => PgsProgresso.Value++);

            var resultado = await ConsolidaContas(contas, progress);

            var fim = DateTime.Now;

            AtualizarView(resultado, fim - inicio);

            BtnProcessar.IsEnabled = true;
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = string.Empty;
            PgsProgresso.Value = 0;
        }
        private async Task<string[]> ConsolidaContas(IEnumerable<ContaCliente> contas, IProgress<string> reportadorDeProgresso)
        {
            //Vai ser substituido pelo IProgress
            //var taskSchedulerGui = TaskScheduler.FromCurrentSynchronizationContext();

            var tasks = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {
                    var resultado = r_Servico.ConsolidarMovimentacao(conta);

                    reportadorDeProgresso.Report(resultado);
                    //O código acima irá substituir o código abaixo.

                    //Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler
                    //Task.Factory.StartNew(() => PgsProgresso.Value++, CancellationToken.None, TaskCreationOptions.None, taskSchedulerGui);

                    return resultado;
                })
              );

            return await Task.WhenAll(tasks);

        }

        private void AtualizarView(IEnumerable<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{elapsedTime.Seconds}.{elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }


        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
        }
    }
}