#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <dirent.h>
#include <ctype.h>

#define TAMANHO_MAX_LINHA 350						//so para se ter uma nocao do tamanho maximo que uma linha pode ter

// Note: This function returns a pointer to a substring of the original string.
// If the given string was allocated dynamically, the caller must not overwrite
// that pointer with the returned value, since the original pointer must be
// deallocated using the same allocator with which it was allocated.  The return
// value must NOT be deallocated using free() etc.
char *trimwhitespace(char *str){
	char *end;
	// Trim leading space
	while(isspace(*str)) str++;
	if(*str == 0)  // All spaces?
	return str;
	// Trim trailing space
	end = str + strlen(str) - 1;
	while(end > str && isspace(*end)) end--;
	// Write new null terminator
	*(end+1) = 0;
	return str;
}

// char* concat(char *s1, char *s2)
// {
//     char *result = malloc(strlen(s1)+strlen(s2)+1);//+1 for the zero-terminator
//     //in real code you would check for errors in malloc here
//     strcpy(result, s1);
//     strcat(result, s2);
//     return result;
// }

int main(){

	char caminho[TAMANHO_MAX_LINHA];
// 	struct dirent *pDirent;
//     DIR *pDir;
	puts("\n\n#######################\n#######################");
	FILE *paths = fopen("machinepaths.txt", "r");	//abre, em modo leitura, o ficheiro criado na linha anterior
	if (paths == NULL){  							//se retornar NULL ao abrir o ficheiro termina o programa
		perror("fdopen");
		printf("!!!ERRO a abrir os parametros!!!\n");
		fclose(paths);
		exit(0);
	}
	fgets(caminho, sizeof(caminho), paths);	//copia a primeira linha do ficheiro para a variavel caminho
	char *str = caminho;					//cria um apontador para "caminho", esse apontador vai ser enviado para a função trimwhitespace();
	trimwhitespace(str);					//chama a função trimwhitespace(str);
	char caminho_total[sizeof(caminho)+sizeof("/machineparameters.txt")];
	sprintf(caminho_total, "%s%s", caminho, "/machineparameters.txt");
// 	char *caminho_total = concat(caminho, "/machineparameters.txt");
	printf("caminho_total:%s\n", caminho_total);
   	FILE *parameters_file = fopen(caminho_total, "r");	//abre o ficheiro dos parametros
	if (parameters_file == NULL){  				//se retornar NULL ao abrir o ficheiro termina o programa
		perror("fdopen");
		printf("!!!ERRO a abrir os parametros!!!\n");
		fclose(parameters_file);
		exit(0);
	}

	FILE *newfile = fopen("newfile.csv", "a");	//cria um novo ficheiro ou abre-o se ja existir
	if (newfile == NULL){  						//se retornar NULL ao abrir o ficheiro termina o programa
		perror("fdopen");
		printf("!!!Erro ao abrir .csv!!!\n");
		fclose(newfile);
		exit(0);
	}else
		printf("--novo ficheiro aberto(read-only)\n");

////////////////////////############################////////////////////////

//DETERMINAR A LINGUA DO FICHEIRO

	char PT[] = "  Nome da tabela KO";
	char EN[] = "  KO Table name";
	char DE[] = "  KO Tabellenname";

	char *lingua[3] = {PT, EN, DE};				//texto que queremos procurar
	int idioma = -1;

	char *parametro1 = malloc(TAMANHO_MAX_LINHA), *parametro2 = malloc(TAMANHO_MAX_LINHA), *parametro3 = malloc(TAMANHO_MAX_LINHA), *parametro4 = malloc(TAMANHO_MAX_LINHA), *parametro5 = malloc(TAMANHO_MAX_LINHA);
	char *parametro6 = malloc(TAMANHO_MAX_LINHA), *parametro7 = malloc(TAMANHO_MAX_LINHA), *parametro8 = malloc(TAMANHO_MAX_LINHA), *parametro9 = malloc(TAMANHO_MAX_LINHA);
	char *parametros[9] = {parametro1, parametro2, parametro3, parametro4, parametro5, parametro6, parametro7, parametro8, parametro9};	//texto que queremos procurar

	for (int i = 0 ; i <= 2; i++){
		int TAMANHO_TEXTO = strlen(lingua[i]);	//cria a variavel TAMANHO_TEXTO, cujo valor inicial é o tamanho da linha que estamos a procura
		printf("A tentar determinar o idioma do ficheiro dos parametros\n");
		char linha_idioma[TAMANHO_TEXTO], newline[TAMANHO_MAX_LINHA]; 	//determina que cada linha copiada terá x caracteres

		fseek(parameters_file, 0, SEEK_SET);	//move o cursor para o inicio do ficheiro
// 		printf("--Vou procurar:\n");			//indica o texto que vai procurar
// 		puts(lingua[i]);						//indica a string que vamos procurar
		printf("--A iniciar procura\n");
		unsigned char cnt = 35;					//se ao fim de 35 linhas (valor medio para o sitio onde se encontra o que queremos procurar) nao encontrou entao é porque nao é a lingua deste ficheiro
		while (idioma == -1){					//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
			fgets(linha_idioma, TAMANHO_TEXTO+1, parameters_file);
// 			puts(linha_idioma);										//mostra o que encontrou quando passou no while
			if (linha_idioma[TAMANHO_TEXTO - 1] == '\n')				//se o texto que foi lido termina em \n entao
				linha_idioma[TAMANHO_TEXTO - 1] = '\0';				// isso vai ser substituido por \0
			if (strcmp(linha_idioma, lingua[i]) == 0){				//este é o texto que procuramos, se for igual entra no IF
				fseek(parameters_file, -(TAMANHO_TEXTO-1), SEEK_CUR);	//depois de encontrar poe o cursor no inicio da linha
				fgets(newline, TAMANHO_MAX_LINHA, parameters_file);		//le a linha, desta vez na totalidade
				if (newline[strlen(newline) - 1] == '\n')				//se o texto que foi lido termina em \n entao
					newline[strlen(newline) - 1] = '\0';				// isso vai ser substituido por \0
				printf("--idioma encontrado... --> ");
				idioma = i;						//atribui a variavel o valor do i atual para saber qual foi o idioma que encontrou
				switch (idioma){				//vai imprimir o idioma de acordo com o que foi encontrado
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
			}// else{
// 				printf("--nao encontrou\n");	//se nao encontrou correspondencia avisa o utilizador
// 			}
			cnt --;
// 			printf("Procurou %u vezes\n", (35 - cnt));					//indica ao utilizador quantas vezes procurou o texto neste ficheiro (max 35)
			if (cnt == 0){
				printf("Procurou x vezes e nao detectou idioma correspondente\n");
				break;
			}
	  	}
// 		if (idioma == -1)						//se nao encontrou igual, vai informar o utilizador
// 			printf("--Nao encontrou idioma correspondente :(\n");
		fflush(parameters_file);				//garante que nao ha informação perdida na ligação ao ficheiro
		fflush(newfile);						//igual à linha anterior
		if(idioma != -1)						//se ja encontrou o idioma sai do "for"
			break;
	}
	if (idioma == -1){							//se nao encontrou igual, vai informar o utilizador
			printf("--A determinacao do idioma falhou!\n--O programa vai terminar!\n");
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

////////////////////////############################////////////////////////

//cada linha terá pelo menos 70 caracteres

	char fim[] = "\n", virgula[] = ",", conversao[2] = "", inicio = 0, found = 0;

	for (int i = 0 ; i <= 8; i++){
		int TAMANHO_TEXTO = strlen(parametros[i]);
// 		printf("Vai procurar:");
// 		puts(parametros[i]);						//indica o texto que vai procurar
// 		printf("Com o tamanho: %d\n", TAMANHO_TEXTO);

		char line[TAMANHO_TEXTO]; 					//determina que cada linha copiada terá x caracteres
		inicio = 0;
		found = 0;
		char* end;
		int digit = -1;

		fseek(parameters_file, 0, SEEK_SET);		//move o cursor para o inicio do ficheiro
// 		printf("--A iniciar procura\n");

		while (fgets(line, TAMANHO_TEXTO+1, parameters_file)){				//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
			if (line[TAMANHO_TEXTO - 1] == '\n')	//se o texto que foi lido termina em \n entao
				line[TAMANHO_TEXTO - 1] = '\0';		// isso vai ser substituido por \0
			if (strcmp(line, parametros[i]) == 0){	//este é o texto que procuramos, se for igual entra no IF
// 				printf("--texto encontrado. A copiar...\n");
				fputs(line, newfile);				//adiciona a nova linha ao ficheiro
				fputs(";", newfile);				//passa para a celula a seguir para ser adicionado o valor do parametro
				while (strcmp(fgets(conversao, 2, parameters_file), fim)){	//le 2 caracteres da linha (ex:"1","\n" ou ex:"a","\n") e verifica se nao é \n (ex:"\n","\n") se nao for entao vai continuar, inicia a conversao
					if(inicio == 0)					//se é o primeiro que está a tentar converter entao mostra "inicio de conversao"
						printf("A iniciar conversao do valor do parametro\n");
					digit = strtol(conversao, &end, 10);					//converte 'conversao' para inteiro de base '10', se nao conseguir converter da sinal a variavel end
					if (!*end){						//se conseguiu converter, end =! NULL, avisa o utilizador e envia para o ficheiro
						fprintf(newfile, "%d", digit);						//adiciona o novo digito ao ficheiro (celula)
						printf("adicionou o digito:%d\n", digit);
					}
					if (!strcmp(conversao, ".")){	//pode nao ter encontrado um inteiro, mas os pontos (.) tem que ser alterados para virgulas (,)
						fputs(virgula, newfile);	//escreve uma virgula no ficheiro, em vez do ponto que encontrou
						puts("adicionou virgula\n");
					}
					inicio = 1;						//ja converteu, ou nao, um caracter entao nao vai deixar imprimir "inicio de conversao"
				}
// 				printf("terminou a conversao\n");
// 				printf("--escrita ok!\n");
				found = 1;							//se fez uma comparação com sucesso nao vai
				break;								// deixar entrar no if depois do while
			}
		}
		if (found == 0){							//se no final do ficheiro nao encontrou igual, vai informar o utilizador
			printf("--End of file without match\n");
		}
		fflush(parameters_file);
		fflush(newfile);
		fputs("\n", newfile);						//desce uma linha
	}
	if (found == 1){								//se no final do ficheiro encontrou todos os parametros, informa o utilizador
		printf("--escrita dos parametros ok!\n");
	}
	fputs("\n", newfile);							//desce uma linha para que se corrermos o mesmo codigo sobre o mesmo ficheiro eles nao escreva os parametros "colados" aos anteriores
	fclose(parameters_file);
	fclose(newfile);
	printf("--!Programa vai terminar!\n");
}










