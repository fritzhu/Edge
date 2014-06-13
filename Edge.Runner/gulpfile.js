var gulp = require('gulp'),
    watch = require('gulp-watch'),
    livereload = require('gulp-livereload');

gulp.task('default', function () {

    gulp.src('Web/Content/**/*.*')
        .pipe(watch(function(files) {
            return files.pipe(gulp.dest('bin/Debug/Web/Content/')).pipe(livereload());
        }));
    	
});