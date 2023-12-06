using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
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

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            var contas = r_Repositorio.GetContaClientes();

            var contasQuantidadePorThread = contas.Count() / 4;

            //pega os primeiros elementos
            //var contas_parte1 = contas.Take(contas.Count() / 2);
            var contas_parte1 = contas.Take(contasQuantidadePorThread);


            //pula uma parte dos elementos
            //var contas_parte2 = contas.Skip(contas.Count() / 2);

            var contas_parte2 = contas.Skip(contasQuantidadePorThread).Take(contasQuantidadePorThread);
            var contas_parte3 = contas.Skip(contasQuantidadePorThread * 2).Take(contasQuantidadePorThread);
            var contas_parte4 = contas.Skip(contasQuantidadePorThread * 3);


            var resultado = new List<string>();

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            //delegate é pode ser representado por uma função lambda, somente isso não é suficiente para iniciar a thread, precisa do método Start()
            Thread thread_parte_1 = new Thread(() =>
            {
                foreach (var conta in contas_parte1)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);

                    resultado.Add(resultadoProcessamento);
                }
            });


            Thread thread_parte_2 = new Thread(() =>
            {
                foreach (var conta in contas_parte2)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);

                    resultado.Add(resultadoProcessamento);
                }
            });

            Thread thread_parte_3 = new Thread(() =>
            {
                foreach (var conta in contas_parte3)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);

                    resultado.Add(resultadoProcessamento);
                }
            });

            Thread thread_parte_4 = new Thread(() =>
            {
                foreach (var conta in contas_parte4)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);

                    resultado.Add(resultadoProcessamento);
                }
            });

            Console.WriteLine($"Thread parte 1, {thread_parte_1.ManagedThreadId} {thread_parte_1.ThreadState} {thread_parte_1.IsAlive}");
            Console.WriteLine($"Thread parte 2, {thread_parte_2.ManagedThreadId} {thread_parte_2.ThreadState} {thread_parte_2.IsAlive}");
            Console.WriteLine($"Thread parte 3, {thread_parte_3.ManagedThreadId} {thread_parte_3.ThreadState} {thread_parte_3.IsAlive}");
            Console.WriteLine($"Thread parte 4, {thread_parte_4.ManagedThreadId} {thread_parte_4.ThreadState} {thread_parte_4.IsAlive}");


            thread_parte_1.Start();
            thread_parte_2.Start();
            thread_parte_3.Start();
            thread_parte_4.Start();

            Console.WriteLine($"Thread parte 1, {thread_parte_1.ManagedThreadId} {thread_parte_1.IsAlive}");
            Console.WriteLine($"Thread parte 2, {thread_parte_2.ManagedThreadId} {thread_parte_2.IsAlive}");
            Console.WriteLine($"Thread parte 3, {thread_parte_3.ManagedThreadId} {thread_parte_3.IsAlive}");
            Console.WriteLine($"Thread parte 4, {thread_parte_4.ManagedThreadId} {thread_parte_4.IsAlive}");

            //verifica se a thread terminou ou está em execução
            while (thread_parte_1.IsAlive || thread_parte_2.IsAlive || thread_parte_3.IsAlive || thread_parte_4.IsAlive)
            {
                //Sem o Sleep continua gastando memória, como sleep coloca pra aguardar
                Thread.Sleep(250);
                //Não vai fazer nada... vai ficar esperando até as Threads terminarem
            }

            //código linha a linha
            //foreach (var conta in contas)
            //{
            //    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
            //    resultado.Add(resultadoConta);
            //}

            var fim = DateTime.Now;

            AtualizarView(resultado, fim - inicio);
        }

        private void AtualizarView(List<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{elapsedTime.Seconds}.{elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }


        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
        }
    }
}