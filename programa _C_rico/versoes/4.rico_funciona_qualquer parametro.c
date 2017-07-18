#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define TAMANHO_TEXTO_TOTAL 200

int main(){

	FILE *newfile = fopen("newfile.txt", "a");	//cria um novo ficheiro ou abre-o se ja existir
	printf("--novo ficheiro aberto\n");
	FILE *parameters = fopen("machineparameters.txt", "r"); //abre o ficheiro dos parametros
	char texto[] = "  Modo de chave travada";		//texto que queremos procurar
	char TAMANHO_TEXTO = sizeof(texto);
	char line[TAMANHO_TEXTO], newline[TAMANHO_TEXTO_TOTAL]; 		//determina que cada linha copiada terá x caracteres
	char found = 0;


  	if (parameters == NULL)  //se retornar NULL ao abrir o ficheiro termina o programa
  	{
  		perror("fdopen");
  		printf("!!!ERRO a abrir os parametros!!!\n");
  		fclose(parameters);
  		exit(0);
  	}

//cada linha terá pelo menos 70 caracteres

	printf("--Vou procurar:\n");				//indica o texto que vai procurar
	puts(texto);
	printf("--A iniciar procura\n");
	while (fgets(line, TAMANHO_TEXTO, parameters))//lê uma linha do ficheiro (ou 500 caracteres, o que chegar primeiro)
	{
		puts(line);								//mostra o que encontrou quando passou no while
		if (line[strlen(line) - 1] == '\n')		//se o texto que foi lido termina em \n entao
    		line[strlen(line) - 1] = '\0';		// isso vai ser substituido por \0
		puts(line);								//mostra o que resultou do passo anterior
		printf("--tentativa de comparacao: \n");
		if (strcmp(line, texto) == 0)			//este é o texto que procuramos, se for igual entra no IF
    	{
    		fseek(parameters, -(TAMANHO_TEXTO-1), SEEK_CUR);	//depois de encontrar poe o cursor no inicio da linha
    		fgets(newline, TAMANHO_TEXTO_TOTAL, parameters);	//le a linha, desta vez na totalidade
       		if (newline[strlen(newline) - 1] == '\n')		//se o texto que foi lido termina em \n entao
    			newline[strlen(newline) - 1] = '\0';		// isso vai ser substituido por \0
       		printf("--texto encontrado. A copiar...\n");
			puts(newline);
			fputs(newline, newfile);			//adiciona a nova linha ao ficheiro
			fputs("\n", newfile);
        	printf("--escrita ok!\n--Programa vai terminar\n");
        	found = 1;							//se fez uma comparação com sucesso nao vai
        	break;								// deixar entrar no if depois do while
    	}else{
    		printf("--nao encontrou\n");
    	}
	}
	if (found == 0){							//se nao encontrei igual, vai informar o utilizador		
		printf("--End of file without match\n--Programa vai terminar\n");
	}
	
	fclose(parameters);
	fclose(newfile);


}
