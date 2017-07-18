#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

#define TAMANHO_TEXTO_TOTAL 200
/*
//Geral
#define parametro1 "  Diferença máxima Y1Y2                      "
//Mute
#define parametro2 "  Tolerânc. Alteração Mudo                   "
//Feedback
#define parametro3 "  Referência esq. (Y1)"
#define parametro4 "  Referência esq. (Y2)"
#define parametro5 "  Veloc.busca ref.Y"
//aproximação rápida
#define parametro6 "  P-ganho                                3895"
#define parametro7 "  Valor de fricção da alimentação        3910"
#define parametro8 "  Ganho de velocidade de alimentação     3917"
#define parametro9 "  Ganho de paralelismo                   3914" 
*/

int main(){

	FILE *newfile = fopen("newfile.txt", "a");					//cria um novo ficheiro ou abre-o se ja existir
	printf("--novo ficheiro aberto\n");
	FILE *parameters_file = fopen("machineparameters.txt", "r");//abre o ficheiro dos parametros
	//Geral
	char parametro1[] = "  Diferença máxima Y1Y2                  ";
	//Mute
	char parametro2[] = "  Tolerânc. Alteração Mudo               ";
	//Feedback
	char parametro3[] = "  Referência esq. (Y1)";
	char parametro4[] = "  Referência dir. (Y2)";
	char parametro5[] = "  Veloc.busca ref.Y";
	//aproximação rápida
	char parametro6[] = "  P-ganho                                3895";
	char parametro7[] = "  Valor de fricção da alimentação        3910";
	char parametro8[] = "  Ganho de velocidade de alimentação     3917";
	char parametro9[] = "  Ganho de paralelismo                   3914";

	char *parametros[9] = {parametro1, parametro2, parametro3, parametro4, parametro5, parametro6, parametro7, parametro8, parametro9};	//texto que queremos procurar

	if (parameters_file == NULL)  //se retornar NULL ao abrir o ficheiro termina o programa
	{
		perror("fdopen");
		printf("!!!ERRO a abrir os parametros!!!\n");
		fclose(parameters_file);
		exit(0);
	}

//cada linha terá pelo menos 70 caracteres

	for (int i = 0 ; i <= 9; i++){
		sleep(3);
		int TAMANHO_TEXTO = strlen(parametros[i]);
		printf("Vai procurar: ");
		puts(parametros[i]);
		printf("Com o tamanho: %d", TAMANHO_TEXTO);
		
		printf("\n");
		char line[TAMANHO_TEXTO], newline[TAMANHO_TEXTO_TOTAL]; 	//determina que cada linha copiada terá x caracteres
		char found = 0;

		fseek(parameters_file, 0, SEEK_SET);		//move o cursor para o inicio do ficheiro
		printf("--Vou procurar:\n");				//indica o texto que vai procurar
		puts(parametros[i]);
		printf("--A iniciar procura\n");
		while (fgets(line, TAMANHO_TEXTO+1, parameters_file))//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
		{
			puts(line);								//mostra o que encontrou quando passou no while
			if (line[strlen(line) - 1] == '\n')		//se o texto que foi lido termina em \n entao
    			line[strlen(line) - 1] = '\0';		// isso vai ser substituido por \0
			puts(line);								//mostra o que resultou do passo anterior
			printf("--tentativa de comparacao: \n");
			if (strcmp(line, parametros[i]) == 0)	//este é o texto que procuramos, se for igual entra no IF
    		{
    			fseek(parameters_file, -(TAMANHO_TEXTO-1), SEEK_CUR);	//depois de encontrar poe o cursor no inicio da linha
    			fgets(newline, TAMANHO_TEXTO_TOTAL, parameters_file);	//le a linha, desta vez na totalidade
    	   		if (newline[strlen(newline) - 1] == '\n')				//se o texto que foi lido termina em \n entao
    				newline[strlen(newline) - 1] = '\0';				// isso vai ser substituido por \0
    	   		printf("--texto encontrado. A copiar...\n");
				puts(newline);						//imprime a nova linha
				fputs(newline, newfile);			//adiciona a nova linha ao ficheiro
				fputs("\n", newfile);
    	    	printf("--escrita ok!\n--Programa vai terminar\n");
    	    	found = 1;							//se fez uma comparação com sucesso nao vai
    	    	break;								// deixar entrar no if depois do while
    		}else{
    			printf("--nao encontrou\n");		//se nao encontrou correspondencia avisa o 
    		}
		}
		if (found == 0){							//se nao encontrei igual, vai informar o utilizador		
			printf("--End of file without match\n--!!!!!!!!!!!!!!!!Programa vai terminar\n");
		}
		fflush(parameters_file);
		fflush(newfile);
	}		
	fclose(parameters_file);
	fclose(newfile);

}
