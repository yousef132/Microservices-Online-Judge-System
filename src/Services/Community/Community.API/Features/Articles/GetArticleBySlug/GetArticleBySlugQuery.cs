using Community.API.Common.DTOs;
using MediatR;

namespace Community.API.Features.Articles.GetArticleBySlug;

public record GetArticleBySlugQuery(string Slug) : IRequest<ArticleDto?>;
