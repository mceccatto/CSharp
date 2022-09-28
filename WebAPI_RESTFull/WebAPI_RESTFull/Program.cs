using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json.Nodes;

namespace WebAPI_RESTFull
{
    class Program
    {
        static string saudacao()
        {
            return "Trabalho WebAPI_RESTFull";
        }

        public static string GeraHash(string senha)
        {
            byte[] senhaBytes = System.Text.Encoding.UTF8.GetBytes(senha);
            byte[] hashBytes = SHA256.Create().ComputeHash(senhaBytes);
            string hashString = BitConverter.ToString(hashBytes);
            hashString = hashString.Replace("-", String.Empty);
            return hashString;
        }

        record DadosCandidatoCadastro(string nome, string telefone, string email, string senha);
        record DadosCandidatoAlteracao(long id, string nome, string telefone, string email, string senha, bool status);
        record DadosCandidatoExclusao(long id);
        record DadosEnderecoCadastro(long candidatoId, string logradouro, string numero, string complemento, string cep, string cidade, string estado);
        record DadosSobreCadastro(long candidatoId, string sobre);
        record DadosConhecimentoCadastro(long candidatoId, string conhecimentos);
        record DadosConhecimentoExclusao(long candidatoId);
        record DadosExperienciaCadastro(long candidatoId, string empresa, string cargo, string descricao, string contratacao, string desligamento);
        record DadosExperienciaEditar(long id, long candidatoId, string empresa, string cargo, string descricao, string contratacao, string desligamento);
        record DadosExperienciaExclusao(long id);
        record DadosCertificadoCadastro(long candidatoId, string instituicao, string conteudo);
        record DadosCertificadoEditar(long id, long candidatoId, string instituicao, string conteudo);
        record DadosCertificadoExclusao(long id);

        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            CandidatoDatabase candidatoDatabase = new CandidatoDatabase("CandidatoDatabase.txt");
            EnderecoDatabase enderecoDatabase = new EnderecoDatabase("EnderecoDatabase.txt");
            SobreDatabase sobreDatabase = new SobreDatabase("SobreDatabase.txt");
            ConhecimentoDatabase conhecimentoDatabase = new ConhecimentoDatabase("ConhecimentoDatabase.txt");
            ExperienciaDatabase experienciaDatabase = new ExperienciaDatabase("ExperienciaDatabase.txt");
            CertificadoDatabase certificadoDatabase = new CertificadoDatabase("CertificadoDatabase.txt");

            //exibe saudacao na pagina inicial
            app.MapGet("/", saudacao);

            //inserir candidato
            var inserirCandidato = (DadosCandidatoCadastro dados) => {
                /*
                {
                  "Nome": "string",
                  "Telefone": "string",
                  "Email": "string",
                  "Senha": "string"
                }
                */
                if(dados.nome == "")
                {
                    return "O nome do candidato não foi informado!";
                }
                if (dados.telefone == "")
                {
                    return "O telefone do candidato não foi informado!";
                }
                if (dados.email == "")
                {
                    return "O e-mail do candidato não foi informado!";
                }
                if (dados.senha == "")
                {
                    return "A senha do candidato não foi informado!";
                }
                var candidato = new Candidato(dados.nome, dados.telefone, dados.email, GeraHash(dados.senha));
                try
                {
                    candidatoDatabase.InserirCandidato(candidato);
                    return "Candidato inserido com sucesso!";
                } catch(Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/candidato/cadastrar", inserirCandidato);

            //listar candidatos
            var listarCandidatos = () => {
                return candidatoDatabase.SerializarJson();
            };
            app.MapGet("/candidatos", listarCandidatos);

            //buscar candidato pelo e-mail
            var buscaCandidato = (string email) => {
                try
                {
                    return candidatoDatabase.BuscaCandidatoEmail(email);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapGet("/candidato/{email}", buscaCandidato);

            //buscar candidato pelo e-mail
            var buscaCandidatoCompleto = (string email) => {
                string dadosCandidato, json;

                try
                {
                    dadosCandidato = candidatoDatabase.BuscaCandidatoEmail(email);
                    Candidato? candidato = JsonConvert.DeserializeObject<Candidato>(dadosCandidato);
                    if (candidato != null)
                    {
                        json = "{";
                        json += "'Nome':'" + candidato.Nome + "',";
                        json += "'Telefone':'" + candidato.Telefone + "',";
                        json += "'Email':'" + candidato.Email + "',";
                        try
                        {
                            json += "'Endereco':";
                            Endereco? endereco = JsonConvert.DeserializeObject<Endereco>(enderecoDatabase.BuscaEnderecoId(candidato.Id));
                            if (endereco != null)
                            {
                                json += "[{";
                                json += "'Logradouro':'" + endereco.Logradouro + "',";
                                json += "'Numero':'" + endereco.Numero + "',";
                                json += "'Complemento':'" + endereco.Complemento + "',";
                                json += "'Cep':'" + endereco.Cep + "',";
                                json += "'Cidade':'" + endereco.Cidade + "',";
                                json += "'Estado':'" + endereco.Estado + "'";
                                json += "}],";
                            }
                            else
                            {
                                json += "'',";
                            }
                        }
                        catch (Exception)
                        {
                            json += "'Candidato não possui endereço cadastrado no sistema.',";
                            //return ex.Message;
                        }
                        try
                        {
                            Sobre? sobre = JsonConvert.DeserializeObject<Sobre>(sobreDatabase.BuscaSobreId(candidato.Id));
                            if (sobre != null)
                            {
                                json += "'Sobre':'" + sobre.Descricao + "',";
                            }
                            else
                            {
                                json += "'Sobre':'',";
                            }
                        }
                        catch (Exception)
                        {
                            json += "'Sobre':'Candidato não possui conteúdo sobre ele cadastrado no sistema.',";
                            //return ex.Message;
                        }
                        try
                        {
                            json += "'Conhecimentos':";
                            Conhecimento? conhecimento = JsonConvert.DeserializeObject<Conhecimento>(conhecimentoDatabase.BuscaConhecimentoId(candidato.Id));
                            if (conhecimento != null)
                            {
                                string[] conhecimentos = conhecimento.Conhecimentos.Split(";");
                                json += "[";
                                for (int i = 0; i < conhecimentos.Length; i++)
                                {
                                    conhecimentos[i] = conhecimentos[i].Trim();
                                    if (i != (conhecimentos.Length - 1))
                                    {
                                        json += "'" + conhecimentos[i] + "',";
                                    }
                                    else
                                    {
                                        json += "'" + conhecimentos[i] + "'";
                                    }
                                }
                                json += "],";
                            }
                            else
                            {
                                json += "'',";
                            }
                        }
                        catch (Exception)
                        {
                            json += "'Candidato não possui conhecimentos cadastrados no sistema.',";
                            //return ex.Message;
                        }
                        try
                        {
                            json += "'Experiencias':";
                            string experiencias = experienciaDatabase.BuscaExperienciaId(candidato.Id);
                            string[] experienciaArray = experiencias.Split(";");
                            if(experienciaArray.Length > 1)
                            {
                                for (int i = 0; i < experienciaArray.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        json += "[";
                                    }
                                    if (i != (experienciaArray.Length - 1))
                                    {
                                        json += experienciaArray[i] + ",";
                                    }
                                    else
                                    {
                                        json += experienciaArray[i];
                                        json += "],";
                                    }
                                }
                            } else
                            {
                                json += "'Candidato não possui nenhuma experiencia cadastrada no sistema.',";
                            }
                        }
                        catch (Exception)
                        {
                            json += "'Candidato não possui nenhuma experiencia cadastrada no sistema.',";
                            //return ex.Message;
                        }
                        try
                        {
                            json += "'Certificados':";
                            string certificados = certificadoDatabase.BuscaCertificadoId(candidato.Id);
                            string[] certificadoArray = certificados.Split(",");
                            if(certificadoArray.Length > 1)
                            {
                                for (int i = 0; i < certificadoArray.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        json += "[";
                                    }
                                    if (i != (certificadoArray.Length - 1))
                                    {
                                        json += certificadoArray[i] + ",";
                                    }
                                    else
                                    {
                                        json += certificadoArray[i];
                                        json += "]";
                                    }
                                }
                                
                            } else
                            {
                                json += "'Candidato não possui nenhuma certificado cadastrado no sistema.'";
                            }
                            
                        }
                        catch (Exception)
                        {
                            json += "'Candidato não possui nenhuma certificado cadastrado no sistema.'";
                            //return ex.Message;
                        }
                        json += "}";
                    }
                    else
                    {
                        return "Informações inválidas.";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

                JObject jsonObj = JObject.Parse(json);

                return JsonConvert.SerializeObject(jsonObj);
            };
            app.MapGet("/candidato/completo/{email}", buscaCandidatoCompleto);

            //edicao de dados do candidato
            var editarCandidato = (DadosCandidatoAlteracao dados) => {
                /*
                {
                  "Id": long,
                  "Nome": "string",
                  "Telefone": "string",
                  "Email": "string",
                  "Senha": "string",
                  "Status": bool
                }
                */
                if (dados.nome == "")
                {
                    return "O nome do candidato não foi informado!";
                }
                if (dados.telefone == "")
                {
                    return "O telefone do candidato não foi informado!";
                }
                if (dados.email == "")
                {
                    return "O e-mail do candidato não foi informado!";
                }
                if (dados.senha == "")
                {
                    return "A senha do candidato não foi informado!";
                }
                var candidato = new Candidato(dados.id, dados.nome, dados.telefone, dados.email, GeraHash(dados.senha), dados.status);
                try
                {
                    candidatoDatabase.AtualizarCandidato(candidato);
                    return "Candidato atualizado com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/candidato/editar", editarCandidato);

            //exclusao de candidato
            var excluirCandidato = (DadosCandidatoExclusao dados) => {
                /*
                {
                  "Id": long
                }
                */
                try
                {
                    candidatoDatabase.RemoverCandidato(dados.id);
                    return "Candidato excluido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/candidato/excluir", excluirCandidato);

            //inserir endereco
            var InserirEndereco = (DadosEnderecoCadastro dados) => {
                /*
                {
	              "CandidatoId": long,
	              "Logradouro": "string",
	              "Numero": "string",
	              "Complemento": "string",
                  "Cep": "string",
                  "Cidade": "string",
                  "Estado": "string"
                }
                */
                if (dados.logradouro == "")
                {
                    return "O logradouro do candidato não foi informado!";
                }
                if (dados.numero == "")
                {
                    return "O numero do endereco do candidato não foi informado!";
                }
                if (dados.complemento == "")
                {
                    return "O complemento do endereco do candidato não foi informado!";
                }
                if (dados.cep == "")
                {
                    return "O cep do endereco do candidato não foi informado!";
                }
                if (dados.cidade == "")
                {
                    return "A cidade do candidato não foi informado!";
                }
                if (dados.estado == "")
                {
                    return "O estado do candidato não foi informado!";
                }
                var endereco = new Endereco(dados.candidatoId, dados.logradouro, dados.numero, dados.complemento, dados.cep, dados.cidade, dados.estado);
                try
                {
                    enderecoDatabase.InserirEndereco(endereco);
                    return "Endereco inserido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/endereco/cadastrar", InserirEndereco);

            //listar enderecos
            var listarEnderecos = () => {
                return enderecoDatabase.SerializarJson();
            };
            app.MapGet("/enderecos", listarEnderecos);

            //buscar endereco do candidato pelo e-mail
            var buscaCandidatoEndereco = (string email) => {
                try
                {
                    var dadosCandidato = candidatoDatabase.BuscaCandidatoEmail(email);
                    Candidato? candidato = JsonConvert.DeserializeObject<Candidato>(dadosCandidato);
                    if(candidato != null)
                    {
                        return enderecoDatabase.BuscaEnderecoId(candidato.Id);
                    } else
                    {
                        return "Informações inválidas.";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapGet("/endereco/candidato/{email}", buscaCandidatoEndereco);

            //edicao de dados do endereco do candidato
            var editarCandidatoEndereco = (DadosEnderecoCadastro dados) => {
                /*
                {
	              "CandidatoId": long,
	              "Logradouro": "string",
	              "Numero": "string",
	              "Complemento": "string",
                  "Cep": "string",
                  "Cidade": "string",
                  "Estado": "string"
                }
                */
                if (dados.logradouro == "")
                {
                    return "O logradouro do candidato não foi informado!";
                }
                if (dados.numero == "")
                {
                    return "O numero do endereco do candidato não foi informado!";
                }
                if (dados.complemento == "")
                {
                    return "O complemento do endereco do candidato não foi informado!";
                }
                if (dados.cep == "")
                {
                    return "O cep do endereco do candidato não foi informado!";
                }
                if (dados.cidade == "")
                {
                    return "A cidade do candidato não foi informado!";
                }
                if (dados.estado == "")
                {
                    return "O estado do candidato não foi informado!";
                }
                var endereco = new Endereco(dados.candidatoId, dados.logradouro, dados.numero, dados.complemento, dados.cep, dados.cidade, dados.estado);
                try
                {
                    enderecoDatabase.AtualizarEndereco(endereco);
                    return "Endereco atualizado com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/endereco/candidato/editar", editarCandidatoEndereco);

            //exclusao de endereco
            var excluirEndereco = (DadosCandidatoExclusao dados) => {
                /*
                {
                  "Id": long
                }
                */
                try
                {
                    enderecoDatabase.RemoverEndereco(dados.id);
                    return "Endereco excluido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/endereco/candidato/excluir", excluirEndereco);

            //inserir conteudo sobre o candidato
            var InserirSobre = (DadosSobreCadastro dados) => {
                /*
                {
	              "CandidatoId": long,
	              "Sobre": "string"
                }
                */
                if(dados.sobre == "")
                {
                    return "O conteúdo sobre não pode estar em branco!";
                }
                var descricao = new Sobre(dados.candidatoId, dados.sobre);
                try
                {
                    sobreDatabase.InserirSobre(descricao);
                    return "Conteudo sobre o candidato inserido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/sobre/candidato/cadastrar", InserirSobre);

            //listar conteudos sobre os candidatos
            var listarSobre = () => {
                return sobreDatabase.SerializarJson();
            };
            app.MapGet("/sobre", listarSobre);

            //buscar conteudo sobre o candidato pelo e-mail
            var buscaCandidatoSobre = (string email) => {
                try
                {
                    var dadosCandidato = candidatoDatabase.BuscaCandidatoEmail(email);
                    Candidato? candidato = JsonConvert.DeserializeObject<Candidato>(dadosCandidato);
                    if (candidato != null)
                    {
                        return sobreDatabase.BuscaSobreId(candidato.Id);
                    }
                    else
                    {
                        return "Informações inválidas.";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapGet("/sobre/candidato/{email}", buscaCandidatoSobre);

            //edicao de dados do conteudo sobre do candidato
            var editarCandidatoSobre = (DadosSobreCadastro dados) => {
                /*
                {
	              "CandidatoId": long,
	              "Sobre": "string"
                }
                */
                if (dados.sobre == "")
                {
                    return "O conteúdo sobre não pode estar em branco!";
                }
                var descricao = new Sobre(dados.candidatoId, dados.sobre);
                try
                {
                    sobreDatabase.AtualizarSobre(descricao);
                    return "Sobre atualizado com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/sobre/candidato/editar", editarCandidatoSobre);

            //exclusao de conteudo sobre o candidato
            var excluirSobre = (DadosCandidatoExclusao dados) => {
                /*
                {
                  "Id": long
                }
                */
                try
                {
                    sobreDatabase.RemoverSobre(dados.id);
                    return "Conteudo sobre excluido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/sobre/candidato/excluir", excluirSobre);

            //Inserir Conhecimento do candidato
            var InserirConhecimento = (DadosConhecimentoCadastro dados) => {
                /*
                {
                    "candidatoId": long,
                    "conhecimentos": "string" SEPARAR ENTRADA COM ;
                }
                */
                if(dados.conhecimentos == "")
                {
                    return "O conteúdo Conhecimentos não pode estar em branco!";
                }
                var conhecimento = new Conhecimento(dados.candidatoId, dados.conhecimentos);
                try
                {
                    conhecimentoDatabase.InserirConhecimento(conhecimento);
                    return "Conhecimento adicionado com sucesso";
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }

            };
            app.MapPost("/conhecimentos/candidato/cadastrar", InserirConhecimento);

            //listar conhecimentos
            var listarConhecimentos = () => {
                return conhecimentoDatabase.SerializarJson();
            };
            app.MapGet("/conhecimentos", listarConhecimentos);

            //Buscar conhecimento por email
            var buscaCandidatoConhecimento = (string email) => {
                try
                {
                    var dadosCandidato = candidatoDatabase.BuscaCandidatoEmail(email);
                    Candidato? candidato = JsonConvert.DeserializeObject<Candidato>(dadosCandidato);
                    if (candidato != null)
                    {
                        return conhecimentoDatabase.BuscaConhecimentoId(candidato.Id);
                    }
                    else
                    {
                        return "Informações inválidas.";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapGet("/conhecimentos/candidato/{email}", buscaCandidatoConhecimento);
                
            //edicao de dados do conteudo conhecimento do candidato
            var editarCandidatoConhecimento = (DadosConhecimentoCadastro dados) => {
                /*
                {
	              "candidatoId": long,
	              "conhecimentos": "string"
                }
                */
                if (dados.conhecimentos == "")
                {
                    return "O conteúdo conhecimentos não pode estar em branco!";
                }
                var conhecimento = new Conhecimento(dados.candidatoId, dados.conhecimentos);
                try
                {
                    conhecimentoDatabase.AtualizarConhecimento(conhecimento);
                    return "Conhecimento atualizado com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/conhecimentos/candidato/editar", editarCandidatoConhecimento);

            var excluirConhecimento = (DadosConhecimentoExclusao dados) => {
                /*
                {
                  "candidatoId": long
                }
                */
                try
                {
                    conhecimentoDatabase.RemoverConhecimento(dados.candidatoId);
                    return "Conteudo conhecimento excluido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/conhecimentos/candidato/excluir", excluirConhecimento);

            //Inserir experiencia do candidato
            var InserirExperiencia = (DadosExperienciaCadastro dados) => {
                /*
                {
                    "candidatoId": long,
                    "empresa": "string",
                    "cargo": "string",
                    "descricao": "string",
                    "contratacao": "string",
                    "desligamento": "string"
                }
                */
                if(dados.empresa == "")
                {
                    return "O conteúdo empresa não pode estar em branco!";
                }
                if(dados.cargo == "")
                {
                    return "O conteúdo cargo não pode estar em branco!";
                }
                if(dados.descricao == "")
                {
                    return "O conteúdo descricao não pode estar em branco!";
                }
                if(dados.contratacao == "")
                {
                    return "O conteúdo contratacao não pode estar em branco!";
                }
                var experiencia = new Experiencia(dados.candidatoId, dados.empresa, dados.cargo, dados.descricao, dados.contratacao, dados.desligamento);
                try
                {
                    experienciaDatabase.InserirExperiencia(experiencia);
                    return "Experiencia adicionado com sucesso";
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }

            };
            app.MapPost("/experiencias/candidato/cadastrar", InserirExperiencia);

            //listar experiencias
            var listarExperiencias = () => {
                return experienciaDatabase.SerializarJson();
            };
            app.MapGet("/experiencias", listarExperiencias);
               
            //Buscar Experiencia por email
            var buscaCandidatoExperiencia = (string email) => {
                try
                {
                    var dadosCandidato = candidatoDatabase.BuscaCandidatoEmail(email);
                    Candidato? candidato = JsonConvert.DeserializeObject<Candidato>(dadosCandidato);
                    if (candidato != null)
                    {
                        return experienciaDatabase.BuscaExperienciaId(candidato.Id);
                    }
                    else
                    {
                        return "Informações inválidas.";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapGet("/experiencias/candidato/{email}", buscaCandidatoExperiencia);

            //edicao de dados do conteudo experiencia do candidato
            var editarCandidatoExperiencia = (DadosExperienciaEditar dados) => {
                /*
                {
                    "id": long,
                    "candidatoId": long,
                    "empresa": "string",
                    "cargo": "string",
                    "descricao": "string",
                    "contratacao": "string",
                    "desligamento": "string"
                }
                */
                if(dados.empresa == "")
                {
                    return "O conteúdo empresa não pode estar em branco!";
                }
                if(dados.cargo == "")
                {
                    return "O conteúdo cargo não pode estar em branco!";
                }
                if(dados.descricao == "")
                {
                    return "O conteúdo descricao não pode estar em branco!";
                }
                if(dados.contratacao == "")
                {
                    return "O conteúdo contratacao não pode estar em branco!";
                }
                var experiencia = new Experiencia(dados.id, dados.candidatoId, dados.empresa, dados.cargo, dados.descricao, dados.contratacao, dados.desligamento);
                try
                {
                    experienciaDatabase.AtualizarExperiencia(experiencia);
                    return "Experiencia atualizado com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/experiencias/candidato/editar", editarCandidatoExperiencia);

            //excluir dados de experiencia
            var excluirExperiencia = (DadosExperienciaExclusao dados) => {
                /*
                {
                  "id": long
                }
                */
                try
                {
                    experienciaDatabase.RemoverExperiencia(dados.id);
                    return "Conteudo Experiencia excluido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/experiencias/candidato/excluir", excluirExperiencia);

            //Inserindo um certificado do candidato
            var InserirCertificado = (DadosCertificadoCadastro dados) => {
                /*
                {
                    "candidatoId": long,
                    "instituicao": "string",
                    "conteudo": "string"
                }
                */
                if(dados.instituicao == "")
                {
                    return "O conteúdo instituicao não pode estar em branco!";
                }
                if(dados.conteudo == "")
                {
                    return "O conteúdo conteudo não pode estar em branco!";
                }
                var certificado = new Certificado(dados.candidatoId, dados.instituicao, dados.conteudo);
                try
                {
                    certificadoDatabase.InserirCertificado(certificado);
                    return "Certificado adicionado com sucesso";
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }

            };
            app.MapPost("/certificados/candidato/cadastrar", InserirCertificado);
                
            //listar certificados
            var listarCertificados = () => {
                return certificadoDatabase.SerializarJson();
            };
            app.MapGet("/certificados", listarCertificados);
               
            //Buscar Certificado por email
            var buscaCandidatoCertificado = (string email) => {
                try
                {
                    var dadosCandidato = candidatoDatabase.BuscaCandidatoEmail(email);
                    Candidato? candidato = JsonConvert.DeserializeObject<Candidato>(dadosCandidato);
                    if (candidato != null)
                    {
                        return certificadoDatabase.BuscaCertificadoId(candidato.Id);
                    }
                    else
                    {
                        return "Informações inválidas.";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapGet("/certificados/candidato/{email}", buscaCandidatoCertificado);

            //edicao de dados do conteudo certificado do candidato
            var editarCandidatoCertificado = (DadosCertificadoEditar dados) => {
                /*
                {
                    "id": long,
                    "candidatoId": long,
                    "instituicao": "string",
                    "conteudo": "string"
                }
                */
                if(dados.instituicao == "")
                {
                    return "O conteúdo instituicao não pode estar em branco!";
                }
                if(dados.conteudo == "")
                {
                    return "O conteúdo conteudo não pode estar em branco!";
                }
                var certificado = new Certificado(dados.id, dados.candidatoId, dados.instituicao, dados.conteudo);
                try
                {
                    certificadoDatabase.AtualizarCertificado(certificado);
                    return "Certificado atualizado com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/certificados/candidato/editar", editarCandidatoCertificado);

            //excluir dados de certificado
            var excluirCertificado = (DadosCertificadoExclusao dados) => {
                /*
                {
                  "id": long
                }
                */
                try
                {
                    certificadoDatabase.RemoverCertificado(dados.id);
                    return "Conteudo certificado excluido com sucesso!";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            };
            app.MapPost("/certificados/candidato/excluir", excluirCertificado);

            app.Run();
        }
    }
}