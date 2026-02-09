using FirebirdSql.Data.FirebirdClient;
using System;

namespace Estagio
{
    class Programa
    {
        static void Main(string[] args)
        {
            string connString = "User=SYSDBA;Password=masterkey;Database=C:\\Pasta de Trabalho\\SAR\\SAR\\bin\\packed\\scripts\\Meubanco\\A.fdb;DataSource=localhost;Port=4050;"; // pasta aonde esta meu banco

            double saldo = 0; // declarei a variavel saldo como inicial
            bool continuaroperacao = true; //Bool true para sempre mostar as alternativas antes de fechar o codigo



            // INICIO OPERAÇÂO BANCO DE DADOS //

            try // aqui eu uso o try catch para tratar erros que possam ocorrer durante a conexão com o banco de dados ou durante as operações SQL
            {
                using (var conexao = new FbConnection(connString)) // aqui eu crio uma nova conexão com o banco de dados usando a string de conexão que foi definida no inicio do codigo
                {
                    conexao.Open();
                    Console.WriteLine("Conectado com sucesso!");

                    Console.WriteLine("Bem-vindo ao sistema bancário!");

                    Console.WriteLine("\nPossui ID de acesso? (sim/nao)");

                    string resposta2 = Console.ReadLine().ToLower();

                    int novoId; // declarei a variavel novoId para armazenar o id que o usuario vai digitar

                    if (resposta2 == "sim") 
                    {
                        Console.Write("Digite seu ID: ");

                        if (int.TryParse(Console.ReadLine(), out novoId)) // se for sim, ai aqui pede pra digitar o iD
                        {

                            string sqlSelect = "SELECT SALDO FROM CLIENTES WHERE ID = @id"; // aqui eu crio a string sqlSelect para selecionar o saldo da tabela
                                                                                            // clientes onde o id for igual ao que o usuario digitou

                            using (var cmd = new FbCommand(sqlSelect, conexao)) // aqui eu crio um comando SQL usando a string sqlSelect e a conexao que foi aberta no inicio do codigo
                            {
                                cmd.Parameters.AddWithValue("@id", novoId); // aqui eu adiciono o parametro @id com o valor do novoId que o usuario digitou
                                var result = cmd.ExecuteScalar(); // aqui eu executo o comando SQL e armazeno o resultado na variavel result, o ExecuteScalar é usado para retornar um valor unico


                                if (result != null)// aqui eu verifico se o resultado é diferente de null, ou seja, se o id foi encontrado no banco de dados
                                {
                                    saldo = Convert.ToDouble(result);
                                    Console.WriteLine($"ID {novoId} encontrado. Seu saldo atual é R$ {saldo:F2}");
                                }


                                else
                                {
                                    Console.WriteLine("ID não encontrado no banco, sistema sera fechado contra fraudes.");
                                    return;
                                }
                            }
                        }


                        else
                        {
                            Console.WriteLine("ID inválido.");
                            return;
                        }
                    }


                    else
                    {

                        Console.Write("Digite o ID que foi informado pelo caixa eletronico: ");
                        if (int.TryParse(Console.ReadLine(), out novoId))
                        {
                            Random rnd = new Random();
                            saldo = rnd.Next(100, 5001); // saldo gerando de 100 a 5001 aleatório na hora da criação do Id 

                            string sqlInsert = "INSERT INTO CLIENTES (ID, SALDO) VALUES (@id, @saldo)"; //Insere dados na tabela clientes, o id e o saldo que foi gerado aleatoriamente
                            using (var cmd = new FbCommand(sqlInsert, conexao)) //enviando instruções SQL para o banco de dados, usando a conexao que foi aberta no inicio do codigo
                            {
                                cmd.Parameters.AddWithValue("@id", novoId); //aqui estou adicionando um paramentro
                                cmd.Parameters.AddWithValue("@saldo", saldo); //aqui estou adicionando o saldo que foi gerado aleatoriamente para o cliente
                                cmd.ExecuteNonQuery();
                            }

                            Console.WriteLine($"Cliente {novoId} criado, seu saldo atual é de R$ {saldo:F2}");
                        }
                        else
                        {
                            Console.WriteLine("ID inválido.");
                            return;
                        }
                    }

                    // aqui pra baixo faço a logica toda do programa, como consultar saldo, sacar, depositar e empréstimo)
                    while (continuaroperacao)
                    {
                        Console.WriteLine("\nEscolha uma operação:");
                        Console.WriteLine("1 - Consultar saldo");
                        Console.WriteLine("2 - Sacar dinheiro");
                        Console.WriteLine("3 - Depositar dinheiro");
                        Console.WriteLine("4 - Informações de empréstimo");
                        Console.WriteLine("5 - Sair");
                        Console.Write("Opção: ");

                        string opcao = Console.ReadLine();

                        switch (opcao)
                        {
                            case "1":
                                Console.WriteLine($"Seu saldo atual é: R$ {saldo:F2}");
                                break;

                            case "2":
                                Console.Write("Digite o valor do saque: ");
                                if (double.TryParse(Console.ReadLine(), out double saque))
                                {
                                    if (saque <= saldo)
                                    {
                                        saldo -= saque;
                                        Console.WriteLine("Saque realizado.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Saldo insuficiente!");
                                    }
                                }
                                break;

                            case "3":
                                Console.Write("Digite o valor do depósito: ");
                                if (double.TryParse(Console.ReadLine(), out double deposito))
                                {
                                    saldo += deposito;
                                    Console.WriteLine("Depósito realizado com sucesso!");
                                }
                                break;

                            case "4":
                                Console.WriteLine("Empréstimo de R$10.000,00 disponível para saldos acima de R$5.000,00");
                                if (saldo >= 5000)
                                {
                                    Console.Write("Deseja aceitar? (sim/nao): ");
                                    string resposta = Console.ReadLine().ToLower();
                                    if (resposta == "sim")
                                    {
                                        saldo += 10000;
                                        Console.WriteLine($"Empréstimo concedido! Novo saldo: R$ {saldo:F2}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Empréstimo recusado.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Saldo insuficiente para empréstimo.");
                                }
                                break;

                            case "5":
                                Console.WriteLine("Obrigado por utilizar nosso banco, até logo!");
                                continuaroperacao = false;
                                break;

                            default:
                                Console.WriteLine("Opção inválida. Tente novamente.");
                                break;
                        }

                        // Atualizar saldo no banco sempre que mudar
                        string sqlUpdate = "UPDATE CLIENTES SET SALDO = @saldo WHERE ID = @id";
                        using (var cmd = new FbCommand(sqlUpdate, conexao))
                        {
                            cmd.Parameters.AddWithValue("@saldo", saldo);
                            cmd.Parameters.AddWithValue("@id", novoId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }
        }
    }
}
