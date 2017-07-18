#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

#define TAMANHO_MAX_LINHA 200


int main(){

	FILE *newfile = fopen("newfile.csv", "a");	//cria um novo ficheiro ou abre-o se ja existir
	printf("--novo ficheiro aberto\n");
	FILE *parameters_file = fopen("machineparameters.txt", "r");//abre o ficheiro dos parametros
	
	if (parameters_file == NULL)  				//se retornar NULL ao abrir o ficheiro termina o programa
	{
		perror("fdopen");
		printf("!!!ERRO a abrir os parametros!!!\n");
		fclose(parameters_file);
		exit(0);
	}

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
//		sleep(1);
		int TAMANHO_TEXTO = strlen(lingua[i]);
		printf("A tentar determinar o idioma do ficheiro\n");
		char linha_parametro[TAMANHO_TEXTO], newline[TAMANHO_MAX_LINHA]; 	//determina que cada linha copiada terá x caracteres

		fseek(parameters_file, 0, SEEK_SET);	//move o cursor para o inicio do ficheiro
		printf("--Vou procurar:\n");			//indica o texto que vai procurar
		puts(lingua[i]);
		printf("--A iniciar procura\n");
		unsigned char cnt = 35;
		while (idioma == -1){					//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
			fgets(linha_parametro, TAMANHO_TEXTO+1, parameters_file);
			puts(linha_parametro);							//mostra o que encontrou quando passou no while
			if (linha_parametro[TAMANHO_TEXTO - 1] == '\n')//se o texto que foi lido termina em \n entao
				linha_parametro[TAMANHO_TEXTO - 1] = '\0';	// isso vai ser substituido por \0
			printf("--a comparar...\n");
			if (strcmp(linha_parametro, lingua[i]) == 0){	//este é o texto que procuramos, se for igual entra no IF
				fseek(parameters_file, -(TAMANHO_TEXTO-1), SEEK_CUR);	//depois de encontrar poe o cursor no inicio da linha
				fgets(newline, TAMANHO_MAX_LINHA, parameters_file);		//le a linha, desta vez na totalidade
				if (newline[strlen(newline) - 1] == '\n')				//se o texto que foi lido termina em \n entao
					newline[strlen(newline) - 1] = '\0';				// isso vai ser substituido por \0
				printf("--idioma correspondente encontrado... --> ");
				idioma = i;						//atribui a variavel o valor do i atual para saber qual foi o idioma que encontrou
				switch (idioma){				//vai imprimir o idioma de acordo com o que foi encontrado
					case 0: printf("PT\n");
						break;
					case 1: printf("EN\n");
						break;
					default: printf("Falhou a representacao do idioma");//se nao for nenhuma das alternativas anteriores avisa o utilizador que
  						break;					// apesar de ter encontrado uma correspondencia nao conseguiu identifica-la corretamente
				}
				break;							//se encontrou correspondencia sai do "while"
			}else{
				printf("--nao encontrou\n");	//se nao encontrou correspondencia avisa o utilizador
			}
			cnt --;
			printf("Procurou %u vezes\n", (35 - cnt));
			if (cnt == 0)
				break;
	  	}
		if (idioma == -1)						//se nao encontrou igual, vai informar o utilizador		
			printf("--Nao encontrou idioma correspondente :(\n");
		fflush(parameters_file);
		fflush(newfile);
		if(idioma != -1)						//se ja encontrou o idioma sai do "for"
			break;
	}
	if (idioma == -1){						//se nao encontrou igual, vai informar o utilizador		
			printf("--A determinacao do idioma falhou!\n--O programa vai terminar!\n");
			exit(0);
		}
	
	switch(idioma){
		case 0: printf("Idioma PT ativado\n");
			//Geral
				strcpy(parametro1, "  Diferença máxima Y1Y2");
			//Mute
				strcpy(parametro2, "  Tolerânc. Alteração Mudo");
			//Feedback
				strcpy(parametro3, "  Referência esq. (Y1)");
				strcpy(parametro4, "  Referência dir. (Y2)");
				strcpy(parametro5, "  Veloc.busca ref.Y");
			//aproximação rápida
				strcpy(parametro6, "  P-ganho                                3895");
				strcpy(parametro7, "  Valor de fricção da alimentação        3910");
				strcpy(parametro8, "  Ganho de velocidade de alimentação     3917");
				strcpy(parametro9, "  Ganho de paralelismo                   3914");
      	break;
    	case 1: printf("Idioma EN ativado\n");
			//Geral
				strcpy(parametro1, "  Maximum Y1Y2 difference");
			//Mute
				strcpy(parametro2, "  Mute changed tolerance");
			//Feedback
				strcpy(parametro3, "  Reference left (Y1)");
				strcpy(parametro4, "  Reference right (Y2)");
				strcpy(parametro5, "  Y-ref search speed");
			//aproximação rápida
				strcpy(parametro6, "  P-gain                                 3895");
				strcpy(parametro7, "  Feedforward Friction value             3910");
				strcpy(parametro8, "  Feedforward Speed gain                 3917");
				strcpy(parametro9, "  Parallelism gain                       3914");
      	break;
      	case 2: printf("Idioma DE ativado\n");
			//Geral
				strcpy(parametro1, "  Maximale Y1Y2-Differenz");
			//Mute
				strcpy(parametro2, "  Übergangsp. geänderte");
			//Feedback
				strcpy(parametro3, "  Referenz links (Y1)");
				strcpy(parametro4, "  Referenz rechts (Y2)");
				strcpy(parametro5, "  Y-Ref Such Geschwindigkeit");
			//aproximação rápida
				strcpy(parametro6, "  P-Verstärkung                          3895");
				strcpy(parametro7, "  Reibungswert Zufuhr vorwärts           3910");
				strcpy(parametro8, "  Vorwärtszufuhr Geschwindigkeit Verstärkung  3917");
				strcpy(parametro9, "  Parallelismus Verstärkung              3914");
      	break;
	}


/*	
//Geral
	char parametro1[] = "  Diferença máxima Y1Y2";
//Mute
	char parametro2[] = "  Tolerânc. Alteração Mudo";
//Feedback
	char parametro3[] = "  Referência esq. (Y1)";
	char parametro4[] = "  Referência dir. (Y2)";
	char parametro5[] = "  Veloc.busca ref.Y";
//aproximação rápida
	char parametro6[] = "  P-ganho                                3895";
	char parametro7[] = "  Valor de fricção da alimentação        3910";
	char parametro8[] = "  Ganho de velocidade de alimentação     3917";
	char parametro9[] = "  Ganho de paralelismo                   3914";


//Geral
	char parametro1[] = "  Maximum Y1Y2 difference";
//Mute
	char parametro2[] = "  Mute changed tolerance";
//Feedback
	char parametro3[] = "  Reference left (Y1)";
	char parametro4[] = "  Reference right (Y2)";
	char parametro5[] = "  Y-ref search speed";
//aproximação rápida
	char parametro6[] = "  P-gain                                 3895";
	char parametro7[] = "  Feedforward Friction value             3910";
	char parametro8[] = "  Feedforward Speed gain                 3917";
	char parametro9[] = "  Parallelism gain                       3914";
*/	

//cada linha terá pelo menos 70 caracteres


	fputs("\n", newfile);							//inicia a escrita no ficheiro com um "enter" para garantir espaco para os
													//parametros anteriormente escritos (se é que havia parametros anteriores)
	for (int i = 0 ; i <= 8; i++){
//		sleep(1);
		int TAMANHO_TEXTO = strlen(parametros[i]);
		printf("Vai procurar:");
		puts(parametros[i]);						//indica o texto que vai procurar
		printf("Com o tamanho: %d\n", TAMANHO_TEXTO);
		
		char line[TAMANHO_TEXTO], newline[TAMANHO_MAX_LINHA]; 	//determina que cada linha copiada terá x caracteres
		char found = 0;

		fseek(parameters_file, 0, SEEK_SET);		//move o cursor para o inicio do ficheiro
//		fputs(parametros[i], newfile);
		printf("--A iniciar procura\n");
		
		while (fgets(line, TAMANHO_TEXTO+1, parameters_file))	//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
		{
//			puts(line);								//mostra o que encontrou quando passou no while
			if (line[TAMANHO_TEXTO - 1] == '\n')	//se o texto que foi lido termina em \n entao
    			line[TAMANHO_TEXTO - 1] = '\0';		// isso vai ser substituido por \0
			puts(line);								//mostra o que resultou do passo anterior
			printf("--tentativa de comparacao: \n");
			if (strcmp(line, parametros[i]) == 0)	//este é o texto que procuramos, se for igual entra no IF
    		{
    	   		printf("--texto encontrado. A copiar...\n");
				fputs(line, newfile);				//adiciona a nova linha ao ficheiro
				fputs(";", newfile);
    			fgets(newline, TAMANHO_MAX_LINHA, parameters_file);		//le a linha, desta vez na totalidade
				fputs(newline, newfile);			//adiciona a nova linha ao ficheiro
//				fseek(parameters_file, -(TAMANHO_TEXTO-1), SEEK_CUR);	//depois de encontrar poe o cursor no inicio da linha
    	    	printf("\n--escrita ok!\n");
    	    	found = 1;							//se fez uma comparação com sucesso nao vai
    	    	break;								// deixar entrar no if depois do while
    		}else{
    			printf("--nao encontrou\n");		//se nao encontrou correspondencia avisa o 
    		}
		}
		if (found == 0){							//se nao encontrei igual, vai informar o utilizador		
			printf("--End of file without match\n");
		}
		fflush(parameters_file);
		fflush(newfile);
	}	
	fclose(parameters_file);
	fclose(newfile);
	printf("--!Programa vai terminar!\n");
}
