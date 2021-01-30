let gulp = require('gulp');
let sass = require('gulp-sass');
let dartCompiler = require('dart-sass');
let rollup = require('rollup');
let { nodeResolve } = require('@rollup/plugin-node-resolve');
const replace = require('@rollup/plugin-replace');
let  { terser } = require('rollup-plugin-terser');

sass.compiler = dartCompiler;
gulp.task('sass', function () {
  return gulp
   .src(__dirname + '/Assets/CSS/main.scss')
   .pipe(sass({
     outputStyle: 'compressed',
    }).on('error', sass.logError))
   .pipe(gulp.dest(__dirname + '/wwwroot/css/'))
});

gulp.task('javascript', () => {
  return rollup.rollup({
    input: 'Assets/JS/main.js',
    plugins: [
      nodeResolve(),
      replace({
        'process.env.NODE_ENV':JSON.stringify("production")
      }),
      terser()
    ]
  }).then(bundle => {
    return bundle.write({
      file: './wwwroot/js/main.js',
      format: 'iife',
      name: 'library',
    });
  });
});

gulp.task('watch', function(cb) {
  gulp.watch(__dirname + '/Assets/CSS/**/*.scss', gulp.series('sass'));
  gulp.watch(__dirname + '/Assets/JS/*.js', gulp.series('javascript'));
  cb();
});
  
gulp.task(
  'default', 
  gulp.parallel('sass', 'javascript', 'watch')
);