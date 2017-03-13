module.exports = function (grunt) {
	grunt.initConfig({
		ts: {
			default: {
				tsconfig: './tsconfig.json'
			}
		},
		browserify: {
			default: {
				src: ['wwwroot/js/src/**/*.js'],
				dest: 'wwwroot/js/dest/compiled.js'
			}
		},
		uglify: {
			js: {
				files: {
					'wwwroot/js/dest/compiled.min.js': ['wwwroot/js/src/**/*.js']
				},
				options: {
					compress: true
				}
			}
		},
		sass: {
			dist: {
				options: {
					style: 'compressed',

				},
				files: {
					'wwwroot/css/main.min.css': 'wwwroot/css/main.scss',
				}
			}
		},
		tslint: {
			options: {
				configuration: "tslint.json"
			},
			files: {
				src: ['wwwroot/app/**/*.ts']
			}
		},
		watch: {
			typescript: {
				files: ['wwwroot/app/**/*.ts'],
				tasks: ['tslint', 'ts', 'browserify']
			},
			sass: {
				files: ['wwwroot/css/*.scss'],
				tasks: ['sass']
			}
		}

	});
	grunt.loadNpmTasks('grunt-ts');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks("grunt-browserify");
	grunt.loadNpmTasks("grunt-contrib-sass");
	grunt.loadNpmTasks("grunt-tslint");

	grunt.registerTask('default', ['tslint', 'ts', 'browserify', 'sass', 'watch']);
	grunt.registerTask('build', ['tslint', 'ts', 'browserify', 'sass']);
};