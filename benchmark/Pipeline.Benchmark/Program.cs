using BenchmarkDotNet.Running;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iterations: " + Config.Iterations);
            new BenchmarkSwitcher(typeof(BenchmarkBase).Assembly).Run(args, new Config());
        }

        public static async Task PipelineStart(int runCount, string nome, int idade)
        {
            var pessoaAcessor = new PessoaAccessor();
            pessoaAcessor.Pessoa = new Pessoa(nome, idade);
            Method(pessoaAcessor, nome, idade, runCount);
            await AsyncMethod(pessoaAcessor, nome, idade, runCount);

            if (nome.Equals(pessoaAcessor.Pessoa?.Nome))
                Console.WriteLine($"Os nomes da {runCount}ª execução não coincidem, o nome esperado é {nome} e o retornado é {pessoaAcessor.Pessoa?.Nome}");
            if (idade.Equals(pessoaAcessor.Pessoa?.Idade))
                Console.WriteLine($"As idades da {runCount}ª execução não coincidem, a idade esperada é {idade} e a retornada é {pessoaAcessor.Pessoa?.Idade}");
        }

        public static void Method(IPessoaAccessor pessoaAccessor, string expectedNome, int expectedIdade, int runCount)
        {
            if (expectedNome.Equals(pessoaAccessor.Pessoa?.Nome))
                Console.WriteLine($"Os nomes da {runCount}ª execução síncrona não coincidem, o nome esperado é {expectedNome} e o retornado é {pessoaAccessor.Pessoa?.Nome}");
            if (expectedIdade.Equals(pessoaAccessor.Pessoa?.Idade))
                Console.WriteLine($"As idades da {runCount}ª execução síncrona não coincidem, a idade esperada é {expectedIdade} e a retornada é {pessoaAccessor.Pessoa?.Idade}");

            pessoaAccessor.Pessoa = new Pessoa("", 0);
        }

        public static async Task AsyncMethod(IPessoaAccessor pessoaAccessor, string expectedNome, int expectedIdade, int runCount)
        {
            await Task.Delay(10);

            if (expectedNome.Equals(pessoaAccessor.Pessoa?.Nome))
                Console.WriteLine($"Os nomes da {runCount}ª execução assíncrona não coincidem, o nome esperado é {expectedNome} e o retornado é {pessoaAccessor.Pessoa?.Nome}");
            if (expectedIdade.Equals(pessoaAccessor.Pessoa?.Idade))
                Console.WriteLine($"As idades da {runCount}ª execução não assíncrona coincidem, a idade esperada é {expectedIdade} e a retornada é {pessoaAccessor.Pessoa?.Idade}");

            pessoaAccessor.Pessoa = new Pessoa("a", 1);
        }
    }

    public interface IPessoaAccessor
    {
        Pessoa Pessoa { get; set; }
    }

    public class PessoaAccessor : IPessoaAccessor
    {
        private static AsyncLocal<PessoaHolder> _pessoaCurrent = new AsyncLocal<PessoaHolder>();

        public Pessoa Pessoa
        {
            get
            {
                return _pessoaCurrent.Value?.Pessoa;
            }
            set
            {
                var holder = _pessoaCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Pessoa = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _pessoaCurrent.Value = new PessoaHolder { Pessoa = value };
                }
            }
        }

        private class PessoaHolder
        {
            public Pessoa Pessoa;
        }
    }

    public class Pessoa
    {
        public string Nome { get; }
        public int Idade { get; }

        public Pessoa(string nome, int idade)
        {
            Nome = nome;
            Idade = idade;
        }
    }
}
