#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <dirent.h>
#include <sys/stat.h>
#include <unistd.h>

#define PATH_MAX 250

typedef void (*ProcessFileFunction)(const char *const);

void findFileByName(const char *const directory, const char *const filename, const char *const originalDirectory, FILE* path_file, ProcessFileFunction process)
{

DIR				*dir;
struct dirent 	*entry;
char           	previousDirectory[PATH_MAX];
char           	currentDirectory[PATH_MAX];

dir = opendir(directory);
if (dir == NULL)
	return;

getcwd(previousDirectory, sizeof(previousDirectory));

chdir(directory);
while ((entry = readdir(dir)) != NULL)
{
	struct stat statbuf;

	if ((strcmp(entry->d_name, ".") == 0) || (strcmp(entry->d_name, "..") == 0))
		continue;
	if (stat(entry->d_name, &statbuf) == -1)
		continue;
	if (S_ISDIR(statbuf.st_mode) != 0)
		findFileByName(entry->d_name, filename, originalDirectory, path_file, process);
	else if (strcmp(filename, entry->d_name) == 0){
			getcwd(currentDirectory, sizeof(currentDirectory));
			printf("\n######\ncurrentDirectory:%s\n", currentDirectory);
			fprintf(path_file, "%s\n", currentDirectory);
			puts("gravou no ficheiro");
			process(entry->d_name);
		}
}
chdir(previousDirectory);
closedir(dir);
}


void processExample(const char *const filename){
    printf("%s\n", filename);
}


int main(){
	char originalDirectory[PATH_MAX];

	FILE* path_file = fopen("machinepaths.txt", "a+");
	if (path_file == NULL){  			//se retornar NULL ao abrir o ficheiro termina o programa
		perror("fdopen");
		printf("!!!ERRO a abrir os parametros!!!\n");
		fclose(path_file);
		exit(0);
	}
	getcwd(originalDirectory, sizeof(originalDirectory));
	puts(originalDirectory);
    findFileByName(".", "machineparameters.txt", originalDirectory, path_file, processExample);
    return 0;
}



