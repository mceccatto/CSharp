using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace WebAPI_RESTFull
{
    class Candidato
    {
        public long Id { get; }
        public string? Nome { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public bool? Status { get; set; }

        public Candidato(string nome, string telefone, string email, string senha)
        {
            this.Id = DateTime.Now.Ticks;
            this.Nome = nome;
            this.Telefone = telefone;
            this.Email = email;
            this.Senha = senha;
            this.Status = true;
        }

        public Candidato(long id, string nome, string telefone, string email, string hash, bool status)
        {
            this.Id = id;
            this.Nome = nome;
            this.Telefone = telefone;
            this.Email = email;
            this.Senha = hash;
            this.Status = status;
        }

        [JsonConstructor]
        public Candidato(long id)
        {
            this.Id = id;
        }

        public string Serializar()
        {
            return this.Id + "," + this.Nome + "," + this.Telefone + "," + this.Email + "," + this.Senha + "," + this.Status;
        }
    }

    class CandidatoDatabase
    {
        string Database;
        List<Candidato> Candidatos;

        public CandidatoDatabase(string arquivo = "CandidatoDatabase.txt")
        {
            this.Database = arquivo;
            this.Candidatos = new List<Candidato>();
            Carregar();
        }

        public void Carregar()
        {
            if (!File.Exists(this.Database))
            {
                File.CreateText(this.Database);
            }
            string carga = File.ReadAllText(this.Database);
            string[] linhas = carga.Split("\n");
            foreach (var linha in linhas)
            {
                if (linha.Length > 0)
                {
                    string[] dados = linha.Split(",");
                    Candidato candidato = new Candidato(long.Parse(dados[0]), dados[1], dados[2], dados[3], dados[4], bool.Parse(dados[5]));
                    this.Candidatos.Add(candidato);
                }
            }
        }

        public void InserirCandidato(Candidato dados)
        {
            foreach (var candidato in this.Candidatos)
            {
                if (candidato.Id == dados.Id)
                {
                    throw new Exception($"O id '{dados.Id}' ja encontra-se registrado no sistema.");
                }
                else if (candidato.Email == dados.Email)
                {
                    throw new Exception($"O e-mail '{dados.Email}' ja encontra-se registrado no sistema.");
                }
            }
            this.Candidatos.Add(dados);
            Salvar();
        }

        public string BuscaCandidatoEmail(string email)
        {
            foreach (var candidato in this.Candidatos)
            {
                if (candidato.Email == email)
                {
                    string json = JsonConvert.SerializeObject(candidato);
                    return json;
                }
            }
            throw new Exception($"Nenhum candidato foi encontrado com o e-mail '{email}'.");
        }

        public void AtualizarCandidato(Candidato dados)
        {
            foreach (var candidato in this.Candidatos)
            {
                if (candidato.Id == dados.Id)
                {
                    this.Candidatos.Remove(candidato);
                    this.Candidatos.Add(dados);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"Nenhum candidato foi encontrado com o Id '{dados.Id}'.");
        }

        public void RemoverCandidato(long id)
        {
            foreach (var candidato in this.Candidatos)
            {
                if (candidato.Id == id)
                {
                    this.Candidatos.Remove(candidato);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"Nenhum candidato foi encontrado com o Id '{id}'.");
        }

        public string BuscaCandidatoId(long id)
        {
            foreach (var candidato in this.Candidatos)
            {
                if (candidato.Id == id)
                {
                    string json = JsonConvert.SerializeObject(candidato);
                    return json;
                }
            }
            throw new Exception($"O candidato o com id '{id}' nao foi encontrado no banco de dados.");
        }

        public string SerializarJson()
        {
            string json = JsonConvert.SerializeObject(this.Candidatos);
            return json;
        }

        public string Serializar()
        {
            string saida = "";
            foreach (var candidato in this.Candidatos)
            {
                saida += candidato.Serializar() + "\n";
            }
            return saida;
        }

        public void Salvar()
        {
            string saida = Serializar();
            File.WriteAllText(this.Database, saida);
        }
    }

    class Endereco
    {
        public long CandidatoId { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Cep { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }

        public Endereco(long candidatoId, string logradouro, string numero, string complemento, string cep, string cidade, string estado)
        {
            this.CandidatoId = candidatoId;
            this.Logradouro = logradouro;
            this.Numero = numero;
            this.Complemento = complemento;
            this.Cep = cep;
            this.Cidade = cidade;
            this.Estado = estado;
        }

        public string Serializar()
        {
            return this.CandidatoId + "," + this.Logradouro + "," + this.Numero + "," + this.Complemento + "," + this.Cep + "," + this.Cidade + "," + this.Estado;
        }
    }

    class EnderecoDatabase
    {
        string Database;
        List<Endereco> Enderecos;

        public EnderecoDatabase(string arquivo = "EnderecoDatabase.txt")
        {
            this.Database = arquivo;
            this.Enderecos = new List<Endereco>();
            Carregar();
        }

        public void Carregar()
        {
            if (!File.Exists(this.Database))
            {
                File.CreateText(this.Database);
            }
            string input = File.ReadAllText(this.Database);
            string[] linhas = input.Split("\n");
            foreach (var linha in linhas)
            {
                if (linha.Length > 0)
                {
                    string[] dados = linha.Split(",");
                    Endereco endereco = new Endereco(long.Parse(dados[0]), dados[1], dados[2], dados[3], dados[4], dados[5], dados[6]);
                    this.Enderecos.Add(endereco);
                }
            }
        }

        public void InserirEndereco(Endereco dados)
        {
            foreach (var endereco in this.Enderecos)
            {
                if (endereco.CandidatoId == dados.CandidatoId)
                {
                    throw new Exception($"O candidato com id '{dados.CandidatoId}' ja possui um endereco registrado no sistema.");
                }
            }
            this.Enderecos.Add(dados);
            Salvar();
        }

        public void AtualizarEndereco(Endereco dados)
        {
            foreach (var endereco in this.Enderecos)
            {
                if (endereco.CandidatoId == dados.CandidatoId)
                {
                    this.Enderecos.Remove(endereco);
                    this.Enderecos.Add(dados);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"O candidato com id '{dados.CandidatoId}' não possui endereco cadastrado.");
        }

        public void RemoverEndereco(long id)
        {
            foreach (var endereco in this.Enderecos)
            {
                if (endereco.CandidatoId == id)
                {
                    this.Enderecos.Remove(endereco);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"Nenhum endereco com Id '{id}' foi encontrado.");
        }

        public string BuscaEnderecoId(long id)
        {
            foreach (var endereco in this.Enderecos)
            {
                if (endereco.CandidatoId == id)
                {
                    string json = JsonConvert.SerializeObject(endereco);
                    return json;
                }
            }
            throw new Exception($"O registro de endere o com id '{id}' nao foi encontrado no banco de dados.");
        }

        public string SerializarJson()
        {
            string json = JsonConvert.SerializeObject(this.Enderecos);
            return json;
        }

        public string Serializar()
        {
            string saida = "";
            foreach (var endereco in this.Enderecos)
            {
                saida += endereco.Serializar() + "\n";
            }
            return saida;
        }

        public void Salvar()
        {
            string saida = Serializar();
            File.WriteAllText(this.Database, saida);
        }
    }

    class Sobre
    {
        public long CandidatoId { get; set; }
        public string Descricao { get; set; }

        public Sobre(long candidatoId, string descricao)
        {
            this.CandidatoId = candidatoId;
            this.Descricao = descricao;
        }

        public string Serializar()
        {
            return this.CandidatoId + "," + this.Descricao;
        }
    }

    class SobreDatabase
    {
        string Database;
        List<Sobre> Descricoes;

        public SobreDatabase(string arquivo = "SobreDatabase.txt")
        {
            this.Database = arquivo;
            this.Descricoes = new List<Sobre>();
            Carregar();
        }

        public void Carregar()
        {
            if (!File.Exists(this.Database))
            {
                File.CreateText(this.Database);
            }
            string input = File.ReadAllText(this.Database);
            string[] linhas = input.Split("\n");
            foreach (var linha in linhas)
            {
                if (linha.Length > 0)
                {
                    string[] dados = linha.Split(",");
                    Sobre descricao = new Sobre(long.Parse(dados[0]), dados[1]);
                    this.Descricoes.Add(descricao);
                }
            }
        }

        public void InserirSobre(Sobre dados)
        {
            foreach (var descricao in this.Descricoes)
            {
                if (descricao.CandidatoId == dados.CandidatoId)
                {
                    throw new Exception($"O candidato com id '{dados.CandidatoId}' ja possui um conteudo sobre registrado no sistema.");
                }
            }
            this.Descricoes.Add(dados);
            Salvar();
        }

        public void AtualizarSobre(Sobre dados)
        {
            foreach (var descricao in this.Descricoes)
            {
                if (descricao.CandidatoId == dados.CandidatoId)
                {
                    this.Descricoes.Remove(descricao);
                    this.Descricoes.Add(dados);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"O candidato com id '{dados.CandidatoId}' não possui um conteudo sobre cadastrado.");
        }

        public void RemoverSobre(long id)
        {
            foreach (var descricao in this.Descricoes)
            {
                if (descricao.CandidatoId == id)
                {
                    this.Descricoes.Remove(descricao);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"Nenhum conteudo sobre com Id '{id}' foi encontrado.");
        }

        public string BuscaSobreId(long id)
        {
            foreach (var descricao in this.Descricoes)
            {
                if (descricao.CandidatoId == id)
                {
                    string json = JsonConvert.SerializeObject(descricao);
                    return json;
                }
            }
            throw new Exception($"O registro de endere o com id '{id}' nao foi encontrado no banco de dados.");
        }

        public string SerializarJson()
        {
            string json = JsonConvert.SerializeObject(this.Descricoes);
            return json;
        }

        public string Serializar()
        {
            string saida = "";
            foreach (var endereco in this.Descricoes)
            {
                saida += endereco.Serializar() + "\n";
            }
            return saida;
        }

        public void Salvar()
        {
            string saida = Serializar();
            File.WriteAllText(this.Database, saida);
        }
    }

    class Conhecimento
    {
        public long CandidatoId { get; set; }
        public string Conhecimentos { get; set; }

        public Conhecimento(long candidatoId, string conhecimentos)
        {
            this.CandidatoId = candidatoId;
            this.Conhecimentos = conhecimentos;
        }

        public string Serializar()
        {
            return this.CandidatoId + "," + this.Conhecimentos;
        }
    }

    class ConhecimentoDatabase
    {
        string Database;
        List<Conhecimento> Conhecimentos;

        public ConhecimentoDatabase(string arquivo = "ConhecimentoDatabase.txt")
        {
            this.Database = arquivo;
            this.Conhecimentos = new List<Conhecimento>();
            Carregar();
        }

        public void Carregar()
        {
            if (!File.Exists(this.Database))
            {
                File.CreateText(this.Database);
            }
            string input = File.ReadAllText(this.Database);
            string[] linhas = input.Split("\n");
            foreach (var linha in linhas)
            {
                if (linha.Length > 0)
                {
                    string[] dados = linha.Split(",");
                    Conhecimento descricao = new Conhecimento(long.Parse(dados[0]), dados[1]);
                    this.Conhecimentos.Add(descricao);
                }
            }
        }

        public void InserirConhecimento(Conhecimento dados)
        {
            foreach (var conhecimento in this.Conhecimentos)
            {
                if (conhecimento.CandidatoId == dados.CandidatoId)
                {
                    throw new Exception($"O candidato com id '{dados.CandidatoId}' ja possui conhecimentos registrado no sistema.");
                }
            }
            this.Conhecimentos.Add(dados);
            Salvar();
        }
        
        public string BuscaConhecimentoId(long id)
        {
            foreach (var conhecimento in this.Conhecimentos)
            {
                if (conhecimento.CandidatoId == id)
                {
                    string json = JsonConvert.SerializeObject(conhecimento);
                    return json;
                }
            }
            throw new Exception($"O registro de conhecimento o com id '{id}' nao foi encontrado no banco de dados.");
        }

        public void AtualizarConhecimento(Conhecimento dados)
        {
            foreach (var conhecimento in this.Conhecimentos)
            {
                if (conhecimento.CandidatoId == dados.CandidatoId)
                {
                    this.Conhecimentos.Remove(conhecimento);
                    this.Conhecimentos.Add(dados);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"O candidato com id '{dados.CandidatoId}' não possui um conteudo de conhecimento cadastrado.");
        }

        public void RemoverConhecimento(long id)
        {
            foreach (var conhecimento in this.Conhecimentos)
            {
                if (conhecimento.CandidatoId == id)
                {
                    this.Conhecimentos.Remove(conhecimento);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"Nenhum conteudo conhecimentos com Id '{id}' foi encontrado.");
        }
        
        public string SerializarJson()
        {
            string json = JsonConvert.SerializeObject(this.Conhecimentos);
            return json;
        }

        public string Serializar()
        {
            string saida = "";
            foreach (var conhecimento in this.Conhecimentos)
            {
                saida += conhecimento.Serializar() + "\n";
            }
            return saida;
        }

        public void Salvar()
        {
            string saida = Serializar();
            File.WriteAllText(this.Database, saida);
        }
    }

    class Experiencia
    {
        public long Id { get; }
        public long CandidatoId { get; set; }
        public string Empresa { get; set; }
        public string Cargo { get; set; }
        public string Descricao { get; set; }
        public string Contratacao { get; set; }
        public string Desligamento { get; set; }

        public Experiencia(long candidatoId, string empresa, string cargo, string descricao, string contratacao, string desligamento)
        {
            this.Id = DateTime.Now.Ticks;
            this.CandidatoId = candidatoId;
            this.Empresa = empresa;
            this.Cargo = cargo;
            this.Descricao = descricao;
            this.Contratacao = contratacao;
            this.Desligamento = desligamento;
        }

        public Experiencia(long id, long candidatoId, string empresa, string cargo, string descricao, string contratacao, string desligamento)
        {
            this.Id = id;
            this.CandidatoId = candidatoId;
            this.Empresa = empresa;
            this.Cargo = cargo;
            this.Descricao = descricao;
            this.Contratacao = contratacao;
            this.Desligamento = desligamento;
        }

        public string Serializar()
        {
            return this.Id + "," + this.CandidatoId + "," + this.Empresa + "," + this.Cargo + "," + this.Descricao + "," + this.Contratacao + "," + this.Desligamento;
        }
    }

    class ExperienciaDatabase
    {
        string Database;
        List<Experiencia> Experiencias;

        public ExperienciaDatabase(string arquivo = "ExperienciaDatabase.txt")
        {
            this.Database = arquivo;
            this.Experiencias = new List<Experiencia>();
            Carregar();
        }

        public void Carregar()
        {
            if (!File.Exists(this.Database))
            {
                File.CreateText(this.Database);
            }
            string input = File.ReadAllText(this.Database);
            string[] linhas = input.Split("\n");
            foreach (var linha in linhas)
            {
                if (linha.Length > 0)
                {
                    string[] dados = linha.Split(",");
                    Experiencia descricao = new Experiencia(long.Parse(dados[0]), long.Parse(dados[1]), dados[2], dados[3], dados[4], dados[5], dados[6]);
                    this.Experiencias.Add(descricao);
                }
            }
        }

        public void InserirExperiencia(Experiencia dados)
        {
            foreach (var experiencia in this.Experiencias)
            {
                if (experiencia.CandidatoId == dados.CandidatoId && experiencia.Empresa == dados.Empresa && experiencia.Cargo == dados.Cargo)
                {
                    throw new Exception($"A experiencia '{experiencia.Id}' ja foi registrada no sistema.");
                }
            }
            this.Experiencias.Add(dados);
            Salvar();
        }

        public string BuscaExperienciaId(long id)
        {
            string json = "";
            foreach (var experiencia in this.Experiencias)
            {
                if (experiencia.CandidatoId == id)
                {
                    json += "{";
                    json += "'Empresa':'" + experiencia.Empresa + "',";
                    json += "'Cargo':'" + experiencia.Cargo + "',";
                    json += "'Descricao':'" + experiencia.Descricao + "',";
                    json += "'Contratacao':'" + experiencia.Contratacao + "',";
                    json += "'Desligamento':'" + experiencia.Desligamento + "'";
                    json += "};";
                }
            }
            return json;
            //TODO Arrumar throw que nunca é chamado
            throw new Exception($"Nenhum registro de experiencia com o CandidatoId '{id}' nao foi encontrado no banco de dados.");
        }

        public void AtualizarExperiencia(Experiencia dados)
        {
            foreach (var experiencia in this.Experiencias)
            {
                if (experiencia.Id == dados.Id)
                {
                    this.Experiencias.Remove(experiencia);
                    this.Experiencias.Add(dados);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"O registro de experiencia com o Id '{dados.Id}' nao foi encontrado no banco de dados.");
        }

        public void RemoverExperiencia(long id)
        {
            foreach (var experiencia in this.Experiencias)
            {
                if (experiencia.Id == id)
                {
                    this.Experiencias.Remove(experiencia);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"Nenhum conteudo de experiencia com Id '{id}' foi encontrado.");
        }

        public string SerializarJson()
        {
            string json = JsonConvert.SerializeObject(this.Experiencias);
            return json;
        }

        public string Serializar()
        {
            string saida = "";
            foreach (var experiencia in this.Experiencias)
            {
                saida += experiencia.Serializar() + "\n";
            }
            return saida;
        }

        public void Salvar()
        {
            string saida = Serializar();
            File.WriteAllText(this.Database, saida);
        }
    }

    class Certificado
    {
        public long Id { get; }
        public long CandidatoId { get; set; }
        public string Instituicao { get; set; }
        public string Conteudo { get; set; }

        public Certificado(long candidatoId, string instituicao, string conteudo)
        {
            this.Id = DateTime.Now.Ticks;
            this.CandidatoId = candidatoId;
            this.Instituicao = instituicao;
            this.Conteudo = conteudo;
        }

        public Certificado(long id, long candidatoId, string instituicao, string conteudo)
        {
            this.Id = id;
            this.CandidatoId = candidatoId;
            this.Instituicao = instituicao;
            this.Conteudo = conteudo;
        }

        public string Serializar()
        {
            return this.Id + "," + this.CandidatoId + "," + this.Instituicao + "," + this.Conteudo;
        }
    }

    class CertificadoDatabase
    {
        string Database;
        List<Certificado> Certificados;

        public CertificadoDatabase(string arquivo = "CertificadoDatabase.txt")
        {
            this.Database = arquivo;
            this.Certificados = new List<Certificado>();
            Carregar();
        }

        public void Carregar()
        {
            if (!File.Exists(this.Database))
            {
                File.CreateText(this.Database);
            }
            string input = File.ReadAllText(this.Database);
            string[] linhas = input.Split("\n");
            foreach (var linha in linhas)
            {
                if (linha.Length > 0)
                {
                    string[] dados = linha.Split(",");
                    Certificado descricao = new Certificado(long.Parse(dados[0]), long.Parse(dados[1]), dados[2], dados[3]);
                    this.Certificados.Add(descricao);
                }
            }
        }

        public void InserirCertificado(Certificado dados)
        {
            foreach (var certificado in this.Certificados)
            {
                if (certificado.CandidatoId == dados.CandidatoId && certificado.Instituicao == dados.Instituicao && certificado.Conteudo == dados.Conteudo)
                {
                    throw new Exception($"O certificado '{certificado.Id}' ja foi registrada no sistema.");
                }
            }
            this.Certificados.Add(dados);
            Salvar();
        }

        public string BuscaCertificadoId(long id)
        {
            string json = "";
            foreach (var certificado in this.Certificados)
            {
                if (certificado.CandidatoId == id)
                {
                    json += "{";
                    json += "'Instituicao':'" + certificado.Instituicao + "',";
                    json += "'Conteudo':'" + certificado.Conteudo + "',";
                    json += "},";
                }
            }
            return json;
            //TODO Arrumar throw que nunca é chamado
            throw new Exception($"Nenhum registro de certificado com o CandidatoId '{id}' nao foi encontrado no banco de dados.");
        }

        public void AtualizarCertificado(Certificado dados)
        {
            foreach (var certificado in this.Certificados)
            {
                if (certificado.Id == dados.Id)
                {
                    this.Certificados.Remove(certificado);
                    this.Certificados.Add(dados);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"O registro de certificado com o Id '{dados.Id}' nao foi encontrado no banco de dados.");
        }

        public void RemoverCertificado(long id)
        {
            foreach (var certificado in this.Certificados)
            {
                if (certificado.Id == id)
                {
                    this.Certificados.Remove(certificado);
                    Salvar();
                    return;
                }
            }
            throw new Exception($"Nenhum conteudo de certificado com Id '{id}' foi encontrado.");
        }

        public string SerializarJson()
        {
            string json = JsonConvert.SerializeObject(this.Certificados);
            return json;
        }

        public string Serializar()
        {
            string saida = "";
            foreach (var certificado in this.Certificados)
            {
                saida += certificado.Serializar() + "\n";
            }
            return saida;
        }

        public void Salvar()
        {
            string saida = Serializar();
            File.WriteAllText(this.Database, saida);
        }

    }
}
