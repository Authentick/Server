let gulp = require('gulp');
let sass = require('gulp-sass');
let dartCompiler = require('dart-sass');

sass.compiler = dartCompiler;
gulp.task('sass', function () {
  return gulp
   .src(__dirname + '/Assets/CSS/main.scss')
   .pipe(sass({
     outputStyle: 'compressed',
    }).on('error', sass.logError))
   .pipe(gulp.dest(__dirname + '/wwwroot/css/'))
});

gulp.task('watch', function(cb) {
    gulp.watch(__dirname + '/Assets/CSS/**/*.scss', gulp.series('sass'));
    cb();
  });
  
  gulp.task(
    'default', 
    gulp.parallel('sass', 'watch')
  );