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
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {

            BtnProcessar.IsEnabled = false;

            _cts = new CancellationTokenSource();

            var contas = r_Repositorio.GetContaClientes();

            PgsProgresso.Maximum = contas.Count();

            LimparView();

            var inicio = DateTime.Now;

            BtnCancelar.IsEnabled = true;

            // Nós criamos o ByteBankProgress mas já existe uma função nativa que faz a mesma coisa,
            //no caso o Progress
            //var byteBankProgress = new ByteBankProgress<string>(str => PgsProgresso.Value++);
            var progress = new Progress<string>(str => PgsProgresso.Value++);

            try
            {
                var resultado = await ConsolidaContas(contas, progress, _cts.Token);

                var fim = DateTime.Now;

                AtualizarView(resultado, fim - inicio);
            }
            catch (OperationCanceledException)
            {
                TxtTempo.Text = "Operação Cancelada pelo usuário!";
            }

            finally
            {
                BtnProcessar.IsEnabled = true;
                BtnCancelar.IsEnabled = false;
            }




        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;

            _cts.Cancel();
        }
        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = string.Empty;
            PgsProgresso.Value = 0;
        }
        private async Task<string[]> ConsolidaContas(IEnumerable<ContaCliente> contas, IProgress<string> reportadorDeProgresso, CancellationToken ct)
        {
            //Vai ser substituido pelo IProgress
            //var taskSchedulerGui = TaskScheduler.FromCurrentSynchronizationContext();

            var tasks = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {

                    //É uma boa prática cancelar no início, para voltar o que foi processado
                    //if (ct.IsCancellationRequested)
                    //    throw new OperationCanceledException(ct);

                    //substitui o código acima
                    ct.ThrowIfCancellationRequested();

                    var resultado = r_Servico.ConsolidarMovimentacao(conta, ct);

                    reportadorDeProgresso.Report(resultado);
                    //O código acima irá substituir o código abaixo.

                    //Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler
                    //Task.Factory.StartNew(() => PgsProgresso.Value++, CancellationToken.None, TaskCreationOptions.None, taskSchedulerGui);

                    ct.ThrowIfCancellationRequested();

                    return resultado;
                }, ct) //Passando esse parâmetro, o taskScheduler sabe que nem
                       //deve iniciar uma tarefa que já foi cancelada
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



    }
}