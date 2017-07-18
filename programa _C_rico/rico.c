#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <dirent.h>
#include <ctype.h>
#include <sys/stat.h>

#define TAMANHO_MAX_LINHA 150
#define PATH_MAX 250

typedef void (*ProcessFileFunction)(const char *const);

// Note: This function returns a pointer to a substring of the original string.
// If the given string was allocated dynamically, the caller must not overwrite
// that pointer with the returned value, since the original pointer must be
// deallocated using the same allocator with which it was allocated.  The return
// value must NOT be deallocated using free() etc.
char *trimwhitespace(char *str){
	char *end;
	while(isspace(*str)) str++;	// Trim leading space
	if(*str == 0)  				// All spaces?
	return str;
	end = str + strlen(str) - 1;// Trim trailing space
	while(end > str && isspace(*end)) end--;
	*(end+1) = 0;				// Write new null terminator
	return str;
}


void findFileByName(const char *const directory, const char *const filename, const char *const originalDirectory, FILE* path_file, ProcessFileFunction process){
	DIR				*dir;
	struct dirent 	*entry;
	char           	previousDirectory[PATH_MAX];
	char           	currentDirectory[PATH_MAX];

	dir = opendir(directory);
	if (dir == NULL)
		return;

	getcwd(previousDirectory, sizeof(previousDirectory));

	chdir(directory);
	while ((entry = readdir(dir)) != NULL){
		struct stat statbuf;
		if ((strcmp(entry->d_name, ".") == 0) || (strcmp(entry->d_name, "..") == 0))
			continue;
		if (stat(entry->d_name, &statbuf) == -1)
			continue;
		if (S_ISDIR(statbuf.st_mode) != 0)
			findFileByName(entry->d_name, filename, originalDirectory, path_file, process);
		else if (strcmp(filename, entry->d_name) == 0){
				getcwd(currentDirectory, sizeof(currentDirectory));
// 				printf("\n######\ncurrentDirectory:%s\n", currentDirectory);
				fprintf(path_file, "%s\n", currentDirectory);
// 				puts("gravou no ficheiro");
				process(entry->d_name);
			}
	}
	chdir(previousDirectory);
	closedir(dir);
}


void processExample(const char *const filename){
//     printf("%s\n", filename);
}



int main(){
	char originalDirectory[PATH_MAX];
	FILE* path_file = fopen("machinepaths.txt", "a+");
	if (path_file == NULL){								//se retornar NULL ao abrir o ficheiro termina o programa
		perror("fdopen");
		printf("!!!ERRO ao abrir o ficheiro de caminhos!!!\n");
		fclose(path_file);
		exit(0);
	}
	getcwd(originalDirectory, sizeof(originalDirectory));
// 	puts(originalDirectory);
	puts("FindFileByName");
	findFileByName(".", "machineparameters.txt", originalDirectory, path_file, processExample);
	fclose(path_file);

// 	#####	Abre o ficheiros dos caminhos
	puts("#######################\n#######################");
	FILE *paths = fopen("machinepaths.txt", "r");		//abre, em modo leitura, o ficheiro criado na linha anterior
	if (paths == NULL){  								//se retornar NULL ao abrir o ficheiro termina o programa
		perror("fdopen");
		printf("!!!ERRO a abrir os caminhos!!!\n");
		fclose(paths);
		exit(0);
	}
//	#####	Prepara e abre o diretorio (caminho) do ficheiro	####
	char caminho[TAMANHO_MAX_LINHA];
	while(fgets(caminho, sizeof(caminho), paths)){		//copia a primeira linha do ficheiro para a variável caminho
		puts("\n\n!!!!__Vai iniciar um novo ficheiro__!!!!");
		char *str = caminho;							//cria um apontador para "caminho", esse apontador vai ser enviado para a função trimwhitespace();
		trimwhitespace(str);							//chama a função trimwhitespace(str);
		char caminho_total[sizeof(caminho)+sizeof("/machineparameters.txt")];
		sprintf(caminho_total, "%s%s", caminho, "/machineparameters.txt");	//atribui à variável "caminho_total" o conteúdo de "caminho" + "/machineparameters.txt"
		printf("caminho_total:%s\n", caminho_total);
		FILE *parameters_file = fopen(caminho_total, "r");	//abre o ficheiro dos parametros
		if (parameters_file == NULL){  						//se retornar NULL ao abrir o ficheiro termina o programa
			perror("fdopen");
			printf("!!!ERRO a abrir os parametros!!!\n");
			fclose(parameters_file);
			exit(0);
		}

////////////////////////############################////////////////////////

	//DETERMINAR A LINGUA DO FICHEIRO

		char PT[] = "  Nome da tabela KO";
		char EN[] = "  KO Table name";
		char DE[] = "  KO Tabellenname";

		char *lingua[3] = {PT, EN, DE};				//texto que queremos procurar
		int idioma = -1, i = 0;

		char *parametro1 = malloc(TAMANHO_MAX_LINHA), *parametro2 = malloc(TAMANHO_MAX_LINHA), *parametro3 = malloc(TAMANHO_MAX_LINHA), *parametro4 = malloc(TAMANHO_MAX_LINHA), *parametro5 = malloc(TAMANHO_MAX_LINHA);
		char *parametro6 = malloc(TAMANHO_MAX_LINHA), *parametro7 = malloc(TAMANHO_MAX_LINHA), *parametro8 = malloc(TAMANHO_MAX_LINHA), *parametro9 = malloc(TAMANHO_MAX_LINHA);
		char *parametros[9] = {parametro1, parametro2, parametro3, parametro4, parametro5, parametro6, parametro7, parametro8, parametro9};	//texto que queremos procurar

		printf("A tentar determinar o idioma do ficheiro dos parametros\n");
		printf("--A iniciar procura\n");
//	##### Vai fazer o ciclo pelos 3 idiomas para tentar descobrir qual é o do ficheiro	#####
		for (i = 0 ; i <= 2; i++){

			int TAMANHO_TEXTO = strlen(lingua[i]);			//cria a variavel TAMANHO_TEXTO, cujo valor inicial é o tamanho da linha que estamos a procura
			char linha_idioma[TAMANHO_TEXTO], newline[TAMANHO_MAX_LINHA]; 	//determina que cada linha copiada terá x caracteres
			unsigned char cnt = 15;							//se ao fim de 35 linhas (valor medio para o sitio onde se encontra o que queremos procurar) nao encontrou entao é porque nao é a lingua deste ficheiro
			rewind(parameters_file);						//move o cursor para o inicio do ficheiro
			while (idioma == -1){							//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
				fgets(linha_idioma, TAMANHO_TEXTO+1, parameters_file);
				if (linha_idioma[TAMANHO_TEXTO - 1] == '\n')				//se o texto que foi lido termina em \n entao
					linha_idioma[TAMANHO_TEXTO - 1] = '\0';					// vamos substituir \n por \0
				fgets(newline, TAMANHO_MAX_LINHA, parameters_file);			//le a linha, desta vez na totalidade
// 				puts(newline);
				if (strcmp(linha_idioma, lingua[i]) == 0){					//este é o texto que procuramos, se for igual entra no IF
					fseek(parameters_file, -(TAMANHO_TEXTO-1), SEEK_CUR);	//depois de encontrar poe o cursor no inicio da linha
					fgets(newline, TAMANHO_MAX_LINHA, parameters_file);		//le a linha, desta vez na totalidade
					if (newline[strlen(newline) - 1] == '\n')				//se o texto que foi lido termina em \n entao
						newline[strlen(newline) - 1] = '\0';				// isso vai ser substituido por \0
					printf("--idioma encontrado... --> ");
					idioma = i;								//atribui a variavel o valor do i atual para saber qual foi o idioma que encontrou
					switch (idioma){						//vai imprimir o idioma de acordo com o que foi encontrado
						case 0: printf("PT\n");
							break;
						case 1: printf("EN\n");
							break;
						case 2: printf("DE\n");
							break;
						default: printf("Falhou a representacao do idioma");//se nao for nenhuma das alternativas anteriores avisa o utilizador que
							break;					// apesar de ter encontrado uma correspondencia nao conseguiu identifica-la corretamente
					}
					break;							//se encontrou correspondencia sai do "while"
				}
				cnt --;
//	#####	Quando falha a determinação do idioma indica qual foi o idioma que falhou	#####
				if (cnt == 0){
					switch (i){
						case 0:
							printf("Procurou o idioma 'Português' e nao correspondeu\n");
							break;
						case 1:
							printf("Procurou o idioma 'Inglês' e nao correspondeu\n");
							break;
						case 2:
							printf("Procurou o idioma 'Alemão' e nao correspondeu\n");
							break;
						default:
							printf("\n");
							break;
					}
				break;
				}
			}
			fflush(parameters_file);				//garante que nao ha informação perdida na ligação ao ficheiro
			if(idioma != -1)						//se ja encontrou o idioma sai do "for"
				break;
		}
		if (idioma == -1){							//se nao encontrou igual, vai informar o utilizador
				printf("--A determinacao do idioma falhou!\n--O programa vai terminar!\n");
				puts("#######################\n#######################");
				exit(0);
			}
		switch(idioma){								//escolhe os parametros que vao ser procurados de acordo com o idioma que foi encontrado
			case 0: printf("Idioma PT ativado\n");
				//Geral
					strcpy(parametro1, "  Diferença máxima Y1Y2                  1496");
				//Mute
					strcpy(parametro2, "  Tolerânc. Alteração Mudo               3148");
				//Feedback
					strcpy(parametro3, "  Referência esq. (Y1)                    454");
					strcpy(parametro4, "  Referência dir. (Y2)                    471");
					strcpy(parametro5, "  Veloc.busca ref.Y                       619");
				//aproximação rápida
					strcpy(parametro6, "  P-ganho                                3895");
					strcpy(parametro7, "  Valor de fricção da alimentação        3910");
					strcpy(parametro8, "  Ganho de velocidade de alimentação     3917");
					strcpy(parametro9, "  Ganho de paralelismo                   3914");
			break;
			case 1: printf("Idioma EN ativado\n");
				//Geral
					strcpy(parametro1, "  Maximum Y1Y2 difference                1496");
				//Mute
					strcpy(parametro2, "  Mute changed tolerance                 3148");
				//Feedback
					strcpy(parametro3, "  Reference left (Y1)                     454");
					strcpy(parametro4, "  Reference right (Y2)                    471");
					strcpy(parametro5, "  Y-ref search speed                      619");
				//aproximação rápida
					strcpy(parametro6, "  P-gain                                 3895");
					strcpy(parametro7, "  Feedforward Friction value             3910");
					strcpy(parametro8, "  Feedforward Speed gain                 3917");
					strcpy(parametro9, "  Parallelism gain                       3914");
			break;
			case 2: printf("Idioma DE ativado\n");
				//Geral
					strcpy(parametro1, "  Maximale Y1Y2-Differenz                1496");
				//Mute
					strcpy(parametro2, "  Übergangsp. geänderte Toleranz         3148");
				//Feedback
					strcpy(parametro3, "  Referenz links (Y1)                     454");
					strcpy(parametro4, "  Referenz rechts (Y2)                    471");
					strcpy(parametro5, "  Y-Ref Such Geschwindigkeit              619");
				//aproximação rápida
					strcpy(parametro6, "  P-Verstärkung                          3895");
					strcpy(parametro7, "  Reibungswert Zufuhr vorwärts           3910");
					strcpy(parametro8, "  Vorwärtszufuhr Geschwindigkeit Verstärkung  3917");
					strcpy(parametro9, "  Parallelismus Verstärkung              3914");
			break;
		}


//	#####	Cria e abre (ou apenas abre) o ficheiro ".csv"	#####
		FILE *excelFile = fopen("newfile.csv", "a");			//cria um novo ficheiro ou abre-o se ja existir
		if (excelFile == NULL){  								//se retornar NULL ao abrir o ficheiro termina o programa
			perror("fdopen");
			printf("!!!Erro ao abrir .csv!!!\n");
			fclose(excelFile);
			exit(0);
		}else
			printf("--ficheiro(.csv) aberto(read-only)\n");


	////////////////////////############################////////////////////////

	//cada linha terá pelo menos 70 caracteres

		char fim[] = "\n", virgula[] = ",", conversao[2] = "", inicio = 0, found = 0;

		for (i = 0 ; i <= 8; i++){
			int TAMANHO_TEXTO = strlen(parametros[i]), digit = -1;
			char line[TAMANHO_TEXTO]; 					//determina que cada linha copiada terá x caracteres
			inicio = 0;
			found = 0;
			char* end;

			fseek(parameters_file, 0, SEEK_SET);		//move o cursor para o inicio do ficheiro

			while (fgets(line, TAMANHO_TEXTO+1, parameters_file)){	//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
				if (line[TAMANHO_TEXTO - 1] == '\n')	//se o texto que foi lido termina em \n entao
					line[TAMANHO_TEXTO - 1] = '\0';		// isso vai ser substituido por \0
				if (strcmp(line, parametros[i]) == 0){	//este é o texto que procuramos, se for igual entra no IF
					fputs(line, excelFile);				//adiciona a nova linha ao ficheiro
					fputs(";", excelFile);				//passa para a celula a seguir para ser adicionado o valor do parametro
					while (strcmp(fgets(conversao, 2, parameters_file), fim)){	//le 2 caracteres da linha (ex:"1","\n" ou ex:"a","\n") e verifica se nao é \n (ex:"\n","\n") se nao for entao vai continuar, inicia a conversao
						if(inicio == 0){				//se é o primeiro que está a tentar converter entao mostra "inicio de conversao"
// 							printf("A iniciar conversao do valor do parametro\n");
						}
						digit = strtol(conversao, &end, 10);		//converte 'conversao' para inteiro de base '10', se nao conseguir converter da sinal a variavel end
						if (!*end){						//se conseguiu converter, end =! NULL, avisa o utilizador e envia para o ficheiro
							fprintf(excelFile, "%d", digit);		//adiciona o novo digito ao ficheiro (celula)
// 							printf("adicionou o digito:%d\n", digit);
						}
						if (!strcmp(conversao, "."))	//pode nao ter encontrado um inteiro, mas os pontos (.) tem que ser alterados para virgulas (,)
							fputs(virgula, excelFile);	//escreve uma virgula no ficheiro, em vez do ponto que encontrou
						inicio = 1;						//ja converteu, ou nao, um caracter entao nao vai deixar imprimir "inicio de conversao"
					}
					found = 1;							//se fez uma comparação com sucesso nao vai
					break;								// deixar entrar no if depois do while
				}
			}
			fflush(parameters_file);
			fflush(excelFile);
			fputs("\n", excelFile);						//desce uma linha
		}
		if (found == 1){								//se no final do ficheiro encontrou todos os parametros, informa o utilizador
			printf("--escrita dos parametros ok!\n");
			fputs(fim, excelFile);
		}
		fputs("\n", excelFile);							//desce uma linha para que se corrermos o mesmo codigo sobre o mesmo ficheiro eles nao escreva os parametros "colados" aos anteriores
		fclose(parameters_file);
		fclose(excelFile);
	}
	puts("Todos os ficheiros verificados");
	fclose(paths);							//fecha o ficheiro dos caminhos dos ficheiros
	printf("--!Programa vai terminar!\n");
	puts("#######################\n#######################");
}










